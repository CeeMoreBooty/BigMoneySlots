using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WelcomeOfferUI : MonoBehaviour
{
    public static WelcomeOfferUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Text Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;

    [Header("Buttons")]
    public Button buyButton;
    public Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        buyButton?.onClick.AddListener(OnBuyClicked);
        closeButton?.onClick.AddListener(Hide);
    }

    private void Start()
    {
        if (WelcomeOfferManager.Instance != null && !WelcomeOfferManager.Instance.IsPurchased)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        BuildDescription();
        panel?.SetActive(true);
    }

    public void Hide()
    {
        panel?.SetActive(false);
    }

    private void OnBuyClicked()
    {
        IAPHandler.Instance?.BuyWelcomeOffer();
    }

    private void BuildDescription()
    {
        var config = WelcomeOfferManager.Instance?.config;
        if (config == null) return;

        if (titleText != null)
            titleText.text = "🎰 WELCOME OFFER — ONE TIME ONLY!";

        if (priceText != null)
            priceText.text = config.priceLabel;

        if (descriptionText != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"💰 {FormatCoins(config.coins)} Coins");
            sb.AppendLine($"🎟️ {config.freeSpins} Free Spins");
            sb.AppendLine($"⭐ {config.superSpins} Super Spins");
            sb.AppendLine($"💎 {config.ultraSpins} Ultra Spin");
            sb.AppendLine($"🎫 {config.jackpotTickets} Jackpot Tickets");
            sb.AppendLine($"🚀 {config.winBoostDurationHours}h {config.winBoostMultiplier}× Win Boost");
            sb.AppendLine($"🏅 VIP Badge");
            sb.AppendLine($"✨ Golden Reel Skin");
            sb.AppendLine($"📦 {config.mysteryChests} Mystery Chests");
            descriptionText.text = sb.ToString();
        }
    }

    private static string FormatCoins(long amount)
    {
        if (amount >= 1_000_000_000_000L) return $"{amount / 1_000_000_000_000L}T";
        if (amount >= 1_000_000_000L)     return $"{amount / 1_000_000_000L}B";
        if (amount >= 1_000_000L)         return $"{amount / 1_000_000L}M";
        if (amount >= 1_000L)             return $"{amount / 1_000L}K";
        return amount.ToString();
    }
}
