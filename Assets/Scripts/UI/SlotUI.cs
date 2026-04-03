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

    [Header("References")]
    public SlotMachine slotMachine;

    private long   _lastCoins     = long.MinValue;
    private long   _lastBet       = long.MinValue;
    private long   _lastJackpot   = long.MinValue;

    private void Awake()
    {
        spinButton?.onClick.AddListener(OnSpinClicked);
    }

    private void OnEnable()
    {
        if (slotMachine != null)
            slotMachine.OnSpinComplete += HandleSpinComplete;
    }

    private void OnDisable()
    {
        if (slotMachine != null)
            slotMachine.OnSpinComplete -= HandleSpinComplete;
    }

    private void Update()
    {
        RefreshHUD();
    }

    private void OnSpinClicked()
    {
        slotMachine?.Spin();
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

    private void RefreshHUD()
    {
        if (PlayerEconomy.Instance != null)
        {
            long coins = PlayerEconomy.Instance.Coins;
            if (coins != _lastCoins)
            {
                _lastCoins = coins;
                if (balanceText != null)
                    balanceText.text = $"💰 {FormatCoins(coins)}";
            }

            if (slotMachine != null)
            {
                long bet = slotMachine.betAmount;
                if (bet != _lastBet)
                {
                    _lastBet = bet;
                    if (betText != null)
                        betText.text = $"Bet: {FormatCoins(bet)}";
                }
            }
        }

        if (ProgressiveJackpot.Instance != null)
        {
            long jackpot = ProgressiveJackpot.Instance.CurrentJackpot;
            if (jackpot != _lastJackpot)
            {
                _lastJackpot = jackpot;
                if (jackpotText != null)
                    jackpotText.text = $"🎰 Jackpot: {FormatCoins(jackpot)}";
            }
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
