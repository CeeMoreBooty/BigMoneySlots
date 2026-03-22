using UnityEngine;

public class IAPHandler : MonoBehaviour
{
    public static IAPHandler Instance { get; private set; }

    private const string ProductIdWelcomeOffer = "welcomeofferpack";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        // TODO: Initialize Unity IAP here.
        // Example using Unity Purchasing:
        //   var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        //   builder.AddProduct(ProductIdWelcomeOffer, ProductType.NonConsumable);
        //   UnityPurchasing.Initialize(this, builder);
        Debug.Log("[IAPHandler] Purchasing initialized (stub). Wire up Unity IAP package.");
    }

    /// <summary>
    /// Called by UI Buy button to start the Google Play purchase flow.
    /// </summary>
    public void BuyWelcomeOffer()
    {
        if (WelcomeOfferManager.Instance != null && WelcomeOfferManager.Instance.IsPurchased)
        {
            Debug.Log("[IAPHandler] Welcome offer already purchased.");
            return;
        }

        // TODO: Replace stub with real Unity IAP call:
        //   m_StoreController.InitiatePurchase(ProductIdWelcomeOffer);
        Debug.Log($"[IAPHandler] Initiating purchase for product: {ProductIdWelcomeOffer}");

#if UNITY_EDITOR
        SimulatePurchaseInEditor();
#endif
    }

    /// <summary>
    /// Called by Unity IAP on successful purchase completion.
    /// Hook this into IStoreListener.ProcessPurchase when wiring up Unity IAP.
    /// </summary>
    public void OnPurchaseCompleted(string productId)
    {
        if (productId == ProductIdWelcomeOffer)
        {
            WelcomeOfferManager.Instance?.GrantWelcomeOffer();
            WelcomeOfferUI.Instance?.Hide();
            Debug.Log("[IAPHandler] Welcome offer purchase complete.");
        }
    }

    /// <summary>
    /// Editor-only simulation so designers can test the full grant flow without a device.
    /// </summary>
    [ContextMenu("Simulate Welcome Offer Purchase (Editor Only)")]
    public void SimulatePurchaseInEditor()
    {
#if UNITY_EDITOR
        Debug.Log("[IAPHandler] Simulating purchase in editor.");
        OnPurchaseCompleted(ProductIdWelcomeOffer);
#endif
    }
}
