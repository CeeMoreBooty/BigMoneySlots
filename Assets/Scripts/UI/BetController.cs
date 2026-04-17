using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for bet selection: decrease, increase, max bet, display.
/// </summary>
public class BetController : MonoBehaviour
{
    [SerializeField] private SlotConfig  slotConfig;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private Button      betMinusButton;
    [SerializeField] private Button      betPlusButton;
    [SerializeField] private Button      maxBetButton;

    private long currentBet;

    public long CurrentBet => currentBet;

    private void Awake()
    {
        currentBet = slotConfig != null ? slotConfig.defaultBet : 500L;
    }

    private void Start()
    {
        betMinusButton?.onClick.AddListener(DecreaseBet);
        betPlusButton?.onClick.AddListener(IncreaseBet);
        maxBetButton?.onClick.AddListener(SetMaxBet);
        RefreshDisplay();
    }

    public void DecreaseBet()
    {
        SoundManager.Instance?.PlayButtonClick();
        long step = slotConfig?.betStep ?? 100;
        long min  = slotConfig?.minBet  ?? 100;
        currentBet = System.Math.Max(min, currentBet - step);
        RefreshDisplay();
    }

    public void IncreaseBet()
    {
        SoundManager.Instance?.PlayButtonClick();
        long step = slotConfig?.betStep ?? 100;
        long max  = slotConfig?.maxBet  ?? 10000;
        currentBet = System.Math.Min(max, currentBet + step);
        RefreshDisplay();
    }

    public void SetMaxBet()
    {
        SoundManager.Instance?.PlayButtonClick();
        long max     = slotConfig?.maxBet  ?? 10000;
        long balance = PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Coins : GameData.Coins;
        currentBet = System.Math.Min(max, balance);
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (betAmountText != null)
            betAmountText.text = CoinDisplay.FormatCoins(currentBet);

        long balance = PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Coins : GameData.Coins;

        // Disable minus if already at minimum
        if (betMinusButton != null)
            betMinusButton.interactable = currentBet > (slotConfig?.minBet ?? 100);

        // Disable plus if at max or can't afford
        if (betPlusButton != null)
        {
            long nextBet = currentBet + (slotConfig?.betStep ?? 100);
            betPlusButton.interactable = nextBet <= (slotConfig?.maxBet ?? 10000)
                                      && nextBet <= balance;
        }
    }
}
