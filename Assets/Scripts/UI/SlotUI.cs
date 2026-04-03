using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("Balance & Bet")]
    public TMP_Text balanceText;
    public TMP_Text betText;

    [Header("Jackpot")]
    public TMP_Text jackpotText;

    [Header("Result")]
    public TMP_Text resultText;

    [Header("Spin Button")]
    public Button spinButton;

    [Header("Auto Spin")]
    public Button autoSpinButton;
    public TMP_Text autoSpinButtonText;

    [Header("References")]
    public SlotMachine slotMachine;

    private void Awake()
    {
        spinButton?.onClick.AddListener(OnSpinClicked);
        autoSpinButton?.onClick.AddListener(OnAutoSpinClicked);
    }

    private void OnEnable()
    {
        if (slotMachine != null)
        {
            slotMachine.OnSpinComplete += HandleSpinComplete;
            slotMachine.OnAutoSpinChanged += HandleAutoSpinChanged;
        }
    }

    private void OnDisable()
    {
        if (slotMachine != null)
        {
            slotMachine.OnSpinComplete -= HandleSpinComplete;
            slotMachine.OnAutoSpinChanged -= HandleAutoSpinChanged;
        }
    }

    private void Update()
    {
        RefreshHUD();
    }

    private void OnSpinClicked()
    {
        slotMachine?.Spin();
    }

    private void OnAutoSpinClicked()
    {
        slotMachine?.ToggleAutoSpin();
    }

    private void HandleSpinComplete(long payout, bool isJackpot)
    {
        if (isJackpot)
            resultText.text = $"🎉 JACKPOT! +{FormatCoins(payout)}";
        else if (payout > 0)
            resultText.text = $"🏆 You won +{FormatCoins(payout)}!";
        else
            resultText.text = "No win. Try again!";
    }

    private void HandleAutoSpinChanged(bool isRunning)
    {
        if (autoSpinButtonText != null)
            autoSpinButtonText.text = isRunning ? "Stop Auto" : "Auto Spin";

        // Disable the manual spin button while auto-spinning so the player
        // uses the Auto Spin button to stop instead.
        if (spinButton != null)
            spinButton.interactable = !isRunning;
    }

    private void RefreshHUD()
    {
        if (PlayerEconomy.Instance != null)
        {
            if (balanceText != null)
                balanceText.text = $"💰 {FormatCoins(PlayerEconomy.Instance.Coins)}";

            if (betText != null && slotMachine != null)
                betText.text = $"Bet: {FormatCoins(slotMachine.betAmount)}";
        }

        if (jackpotText != null && ProgressiveJackpot.Instance != null)
            jackpotText.text = $"🎰 Jackpot: {FormatCoins(ProgressiveJackpot.Instance.CurrentJackpot)}";
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
