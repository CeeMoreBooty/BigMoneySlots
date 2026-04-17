using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Shop scene: displays coin packages, handles IAP and free coin grants.
/// </summary>
public class ShopController : MonoBehaviour, IDetailedStoreListener
{
    [System.Serializable]
    public class CoinPackage
    {
        public string   productId;
        public string   displayName;
        public string   displayPrice;
        public long     coinAmount;
        public bool     isBestValue;
        public Button   buyButton;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI coinsText;
        public TextMeshProUGUI priceText;
        public GameObject      bestValueBadge;
    }

    [Header("Coin Packages")]
    [SerializeField] private CoinPackage[] packages;

    [Header("Free Coins")]
    [SerializeField] private Button watchAdButton;
    [SerializeField] private long   adRewardCoins = 500;

    [Header("Coin Display")]
    [SerializeField] private CoinDisplay coinDisplay;

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    private IStoreController   storeController;
    private IExtensionProvider extensions;
    private bool storeInitialized;

    private void Start()
    {
        SoundManager.Instance?.PlayShopMusic();
        coinDisplay?.Refresh();

        closeButton?.onClick.AddListener(OnClose);
        watchAdButton?.onClick.AddListener(OnWatchAd);

        InitIAP();
        RefreshPackageUI();
    }

    private void InitIAP()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (var pkg in packages)
            builder.AddProduct(pkg.productId, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private void RefreshPackageUI()
    {
        foreach (var pkg in packages)
        {
            if (pkg.nameText  != null) pkg.nameText.text  = pkg.displayName;
            if (pkg.coinsText != null) pkg.coinsText.text = $"{CoinDisplay.FormatCoins(pkg.coinAmount)} COINS";
            if (pkg.priceText != null) pkg.priceText.text = pkg.displayPrice;
            if (pkg.bestValueBadge != null) pkg.bestValueBadge.SetActive(pkg.isBestValue);

            var localPkg = pkg; // capture for lambda
            pkg.buyButton?.onClick.AddListener(() => PurchasePackage(localPkg));
        }
    }

    private void PurchasePackage(CoinPackage pkg)
    {
        SoundManager.Instance?.PlayButtonClick();
        if (storeInitialized && storeController != null)
            storeController.InitiatePurchase(pkg.productId);
        else
            Debug.LogWarning($"Store not ready. Cannot purchase {pkg.productId}");
    }

    private void OnWatchAd()
    {
        SoundManager.Instance?.PlayButtonClick();
        // Simulate ad reward (integrate your ad SDK here)
        GrantCoins(adRewardCoins);
    }

    private void GrantCoins(long amount)
    {
        if (PlayerEconomy.Instance != null)
            PlayerEconomy.Instance.AddCoins(amount);
        else
        {
            GameData.AddCoins(amount);
            GameData.Save();
        }
        coinDisplay?.AnimateTo(CoinDisplay.GetCoins());
        SoundManager.Instance?.PlayCoinDrop();
    }

    private void OnClose()
    {
        SoundManager.Instance?.PlayButtonClick();
        UIManager.Instance?.GoToMainMenu();
    }

    // ── IDetailedStoreListener ───────────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider ext)
    {
        storeController  = controller;
        extensions       = ext;
        storeInitialized = true;

        // Update displayed prices from store metadata
        foreach (var pkg in packages)
        {
            var product = controller.products.WithID(pkg.productId);
            if (product != null && pkg.priceText != null)
                pkg.priceText.text = product.metadata.localizedPriceString;
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
        => Debug.LogWarning($"IAP init failed: {error}");

    public void OnInitializeFailed(InitializationFailureReason error, string message)
        => Debug.LogWarning($"IAP init failed: {error} – {message}");

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        foreach (var pkg in packages)
        {
            if (args.purchasedProduct.definition.id == pkg.productId)
            {
                GrantCoins(pkg.coinAmount);
                Debug.Log($"Purchase complete: {pkg.displayName} → {pkg.coinAmount} coins");
                return PurchaseProcessingResult.Complete;
            }
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        => Debug.LogWarning($"Purchase failed: {product.definition.id} ({failureReason})");

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc)
        => Debug.LogWarning($"Purchase failed: {product.definition.id} – {desc.message}");
}
