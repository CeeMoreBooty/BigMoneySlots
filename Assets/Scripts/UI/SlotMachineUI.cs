using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the slot machine UI: spin button, bet controls, reel display.
/// Attach to the main canvas root.
/// </summary>
public class SlotMachineUI : MonoBehaviour
{
    [Header("Reels (assign Symbol Text arrays per reel)")]
    [SerializeField] private TMP_Text[][] reelTexts; // [reel][row]

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button betUpButton;
    [SerializeField] private Button betDownButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text betLabel;
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text winLabel;

    private SlotMachine slotMachine;
    private UserData    userData;

    private static readonly string[] SymbolEmoji =
    {
        "🍒", "🍋", "🍊", "🍇", "🔔", "📊", "7️⃣", "💎", "⭐"
    };

    private void Awake()
    {
        slotMachine = FindObjectOfType<SlotMachine>();
        userData    = FindObjectOfType<UserData>();
    }

    private void OnEnable()
    {
        if (slotMachine != null)
            slotMachine.OnSpinFinished += HandleSpinFinished;
        if (userData != null)
            userData.OnDataChanged += RefreshCoinsLabel;
    }

    private void OnDisable()
    {
        if (slotMachine != null)
            slotMachine.OnSpinFinished -= HandleSpinFinished;
        if (userData != null)
            userData.OnDataChanged -= RefreshCoinsLabel;
    }

    private void Start()
    {
        if (spinButton)   spinButton.onClick.AddListener(OnSpinClicked);
        if (betUpButton)  betUpButton.onClick.AddListener(OnBetUp);
        if (betDownButton) betDownButton.onClick.AddListener(OnBetDown);

        RefreshBetLabel();
        RefreshCoinsLabel();
        if (winLabel) winLabel.text = "";
    }

    private void OnSpinClicked()
    {
        if (winLabel) winLabel.text = "";
        slotMachine?.Spin();
    }

    private void OnBetUp()
    {
        slotMachine?.IncreaseBet();
        RefreshBetLabel();
    }

    private void OnBetDown()
    {
        slotMachine?.DecreaseBet();
        RefreshBetLabel();
    }

    private void HandleSpinFinished(SlotMachine.Symbol[,] result,
        long payout, SlotMachine.WinLevel winLevel)
    {
        UpdateReelDisplay(result);
        RefreshCoinsLabel();

        if (winLabel)
        {
            winLabel.text = winLevel switch
            {
                SlotMachine.WinLevel.EpicWin => $"🎉 EPIC WIN! +{FormatCoins(payout)}",
                SlotMachine.WinLevel.MegaWin => $"🔥 MEGA WIN! +{FormatCoins(payout)}",
                SlotMachine.WinLevel.BigWin  => $"💥 BIG WIN! +{FormatCoins(payout)}",
                SlotMachine.WinLevel.Normal  => $"+{FormatCoins(payout)}",
                _                            => ""
            };
        }
    }

    private void UpdateReelDisplay(SlotMachine.Symbol[,] result)
    {
        if (reelTexts == null) return;
        int reelCount = result.GetLength(0);
        int rowCount  = result.GetLength(1);
        for (int r = 0; r < reelCount && r < reelTexts.Length; r++)
            for (int row = 0; row < rowCount && reelTexts[r] != null && row < reelTexts[r].Length; row++)
                if (reelTexts[r][row] != null)
                    reelTexts[r][row].text = SymbolEmoji[(int)result[r, row]];
    }

    private void RefreshBetLabel()
    {
        if (betLabel && slotMachine)
            betLabel.text = $"BET: {FormatCoins(slotMachine.CurrentBet)}";
    }

    private void RefreshCoinsLabel()
    {
        if (coinsLabel && userData)
            coinsLabel.text = $"💰 {FormatCoins(userData.Coins)}";
    }

    private static string FormatCoins(long amount)
    {
        if      (amount >= 1_000_000_000_000L) return $"{amount / 1_000_000_000_000.0:0.##}T";
        else if (amount >= 1_000_000_000L)     return $"{amount / 1_000_000_000.0:0.##}B";
        else if (amount >= 1_000_000L)         return $"{amount / 1_000_000.0:0.##}M";
        else if (amount >= 1_000L)             return $"{amount / 1_000.0:0.##}K";
        return amount.ToString();
    }
}
