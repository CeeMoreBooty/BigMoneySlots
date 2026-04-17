using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modal popup shown when the player cannot afford a spin.
/// Offers quick navigation to the Shop or the daily bonus.
/// </summary>
public class InsufficientFundsPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject       panel;
    [SerializeField] private TextMeshProUGUI  balanceText;
    [SerializeField] private TextMeshProUGUI  requiredText;
    [SerializeField] private Button           goToShopButton;
    [SerializeField] private Button           claimDailyButton;
    [SerializeField] private Button           closeButton;

    private void Start()
    {
        goToShopButton?.onClick.AddListener(OnGoToShop);
        claimDailyButton?.onClick.AddListener(OnClaimDaily);
        closeButton?.onClick.AddListener(Hide);
        panel?.SetActive(false);
    }

    /// <summary>Show the popup for a specific required amount.</summary>
    public void Show(long requiredAmount)
    {
        if (balanceText  != null) balanceText.text  = $"Your coins: {CoinDisplay.FormatCoins(CoinDisplay.GetCoins())}";
        if (requiredText != null) requiredText.text  = $"Required: {CoinDisplay.FormatCoins(requiredAmount)}";

        bool canClaim = GameData.CanClaimDailyBonus;
        claimDailyButton?.gameObject.SetActive(canClaim);

        panel?.SetActive(true);
        SoundManager.Instance?.PlayError();
    }

    public void Hide()
    {
        panel?.SetActive(false);
        SoundManager.Instance?.PlayButtonClick();
    }

    private void OnGoToShop()
    {
        Hide();
        UIManager.Instance?.GoToShop();
    }

    private void OnClaimDaily()
    {
        Hide();
        // Trigger the daily bonus controller if it exists in the scene
        FindObjectOfType<DailyBonusController>()?.CheckAndShow();
    }
}
