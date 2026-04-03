using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

/// <summary>
/// Handles Unity IAP initialisation, purchase flow, and server-side receipt verification.
///
/// Products (must match Google Play Console and Backend/routes/payments.js):
///   welcomeofferpack  — NonConsumable — coins + spins + cosmetics bundle
///   gems_100 / _500 / _1200 / _2500 / _6500 — Consumable gem packs
///
/// Purchase flow:
///   1. Player taps Buy → BuyProduct() → UnityPurchasing.InitiatePurchase()
///   2. Google Play processes payment and calls ProcessPurchase().
///   3. We POST the full Unity IAP receipt to /api/payments/unity-iap/verify.
///   4. Backend parses the receipt, verifies with Google, grants items.
///   5. On backend success we call ConfirmPendingPurchase() to close the transaction.
///
/// The #if UNITY_PURCHASING guards let the file compile even before the
/// com.unity.purchasing package resolves in the Unity Package Manager.
/// </summary>
public class IAPHandler : MonoBehaviour
#if UNITY_PURCHASING
    , IStoreListener
#endif
{
    public static IAPHandler Instance { get; private set; }

    // ── Product IDs (must match Play Console + payments.js catalog) ───────────
    public const string ProductWelcomeOffer = "welcomeofferpack";
    public const string ProductGems100      = "gems_100";
    public const string ProductGems500      = "gems_500";
    public const string ProductGems1200     = "gems_1200";
    public const string ProductGems2500     = "gems_2500";
    public const string ProductGems6500     = "gems_6500";

    public bool IsInitialized { get; private set; }

#if UNITY_PURCHASING
    private IStoreController   _storeController;
    private IExtensionProvider _extensions;
#endif

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => InitializePurchasing();

    // ── Initialisation ────────────────────────────────────────────────────────
    private void InitializePurchasing()
    {
#if UNITY_PURCHASING
        if (IsInitialized) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Non-consumable — can only be purchased once per account
        builder.AddProduct(ProductWelcomeOffer, ProductType.NonConsumable);

        // Consumables — can be purchased repeatedly
        builder.AddProduct(ProductGems100,  ProductType.Consumable);
        builder.AddProduct(ProductGems500,  ProductType.Consumable);
        builder.AddProduct(ProductGems1200, ProductType.Consumable);
        builder.AddProduct(ProductGems2500, ProductType.Consumable);
        builder.AddProduct(ProductGems6500, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
        Debug.Log("[IAPHandler] Unity Purchasing initializing...");
#else
        Debug.Log("[IAPHandler] Unity Purchasing package not installed — IAP disabled.");
#endif
    }

    // ── Public purchase API ───────────────────────────────────────────────────

    /// <summary>Initiates a purchase for the welcome offer bundle.</summary>
    public void BuyWelcomeOffer()
    {
        if (WelcomeOfferManager.Instance != null && WelcomeOfferManager.Instance.IsPurchased)
        {
            Debug.Log("[IAPHandler] Welcome offer already purchased.");
            return;
        }
        BuyProduct(ProductWelcomeOffer);
    }

    /// <summary>Initiates a gem pack purchase by product ID.</summary>
    public void BuyGems(string productId) => BuyProduct(productId);

    private void BuyProduct(string productId)
    {
#if UNITY_PURCHASING
        if (!IsInitialized)
        {
            Debug.LogWarning("[IAPHandler] Cannot purchase — store not yet initialized.");
            return;
        }
        var product = _storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log("[IAPHandler] Initiating purchase: " + productId);
            _storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning("[IAPHandler] Product unavailable or not found: " + productId);
        }
#else
        Debug.LogWarning("[IAPHandler] Purchasing disabled — Unity Purchasing package missing.");
#if UNITY_EDITOR
        SimulatePurchaseInEditor(productId);
#endif
#endif
    }

#if UNITY_PURCHASING
    // ── IStoreListener ────────────────────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _extensions      = extensions;
        IsInitialized    = true;
        Debug.Log("[IAPHandler] Unity Purchasing initialized successfully.");
    }

    // Unity IAP 4.x requires both overloads of OnInitializeFailed.
    public void OnInitializeFailed(InitializationFailureReason error)
        => Debug.LogError("[IAPHandler] Initialization failed: " + error);

    public void OnInitializeFailed(InitializationFailureReason error, string message)
        => Debug.LogError("[IAPHandler] Initialization failed: " + error + " — " + message);

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        string receipt   = args.purchasedProduct.receipt;

        Debug.Log("[IAPHandler] Processing purchase: " + productId);

        // Verify with the backend and confirm the transaction once we have a response.
        // Returning Pending keeps the transaction open until ConfirmPendingPurchase().
        StartCoroutine(VerifyReceiptOnServer(productId, receipt));
        return PurchaseProcessingResult.Pending;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        => Debug.LogWarning("[IAPHandler] Purchase failed: " +
                            product.definition.id + " — " + failureReason);
#endif

    // ── Server-side receipt verification ─────────────────────────────────────

    private IEnumerator VerifyReceiptOnServer(string productId, string receipt)
    {
        string url  = BackendClient.BaseUrl + "/api/payments/unity-iap/verify";
        // receipt is already a JSON string from Unity IAP — embed it raw.
        string body = "{\"productId\":\"" + productId + "\",\"receipt\":" +
                      EscapeJsonString(receipt) + "}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        req.timeout = 20;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[IAPHandler] Server verified purchase: " + productId);
            OnPurchaseCompleted(productId);

#if UNITY_PURCHASING
            // Remove the transaction from Unity's pending queue.
            if (_storeController != null)
            {
                var product = _storeController.products.WithID(productId);
                if (product != null) _storeController.ConfirmPendingPurchase(product);
            }
#endif
        }
        else
        {
            Debug.LogError("[IAPHandler] Server verification failed (" + productId + "): " +
                           req.downloadHandler.text);
            // Do NOT confirm — Unity will retry the pending purchase on the next session.
        }
    }

    /// <summary>Applies local rewards after the backend confirms the purchase.</summary>
    public void OnPurchaseCompleted(string productId)
    {
        if (productId == ProductWelcomeOffer)
        {
            WelcomeOfferManager.Instance?.GrantWelcomeOffer();
            WelcomeOfferUI.Instance?.Hide();
            Debug.Log("[IAPHandler] Welcome offer granted.");
        }
        else
        {
            // Gem packs are granted server-side; show a confirmation toast.
            HUDManager.Instance?.ShowToast("Purchase successful! Gems added.", 4f);
            Debug.Log("[IAPHandler] Gem pack purchased: " + productId);
        }
        AnalyticsManager.Instance?.Track(AnalyticsManager.Event.IAPPurchase,
            "{\"productId\":\"" + productId + "\"}");
    }

    // ── Editor simulation ─────────────────────────────────────────────────────

    [ContextMenu("Simulate Welcome Offer Purchase (Editor Only)")]
    public void SimulatePurchaseInEditor()
    {
#if UNITY_EDITOR
        SimulatePurchaseInEditor(ProductWelcomeOffer);
#endif
    }

    private void SimulatePurchaseInEditor(string productId)
    {
#if UNITY_EDITOR
        Debug.Log("[IAPHandler] Simulating purchase in editor: " + productId);
        OnPurchaseCompleted(productId);
#endif
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns <paramref name="s"/> as a properly escaped JSON string literal.</summary>
    private static string EscapeJsonString(string s)
    {
        if (s == null) return "null";
        var sb = new StringBuilder("\"");
        foreach (char c in s)
        {
            switch (c)
            {
                case '"':  sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                default:   sb.Append(c);      break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
