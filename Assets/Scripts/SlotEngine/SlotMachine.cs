using System;
using System.Collections;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [Header("Reels")]
    public Reel[] reels;

    [Header("Config")]
    public PayoutTable payoutTable;
    public long betAmount = 100;

    [Header("Auto Spin")]
    [Tooltip("Seconds between auto spins")]
    public float autoSpinDelay = 1.0f;

    public event Action<long, bool> OnSpinComplete; // (payout, isJackpot)
    public event Action<bool> OnAutoSpinChanged;    // (isRunning)

    public bool IsAutoSpinning { get; private set; }

    private Coroutine _autoSpinCoroutine;

    public void StartAutoSpin()
    {
        if (IsAutoSpinning) return;
        IsAutoSpinning = true;
        _autoSpinCoroutine = StartCoroutine(AutoSpinLoop());
        OnAutoSpinChanged?.Invoke(true);
    }

    public void StopAutoSpin()
    {
        if (!IsAutoSpinning) return;
        IsAutoSpinning = false;
        if (_autoSpinCoroutine != null)
        {
            StopCoroutine(_autoSpinCoroutine);
            _autoSpinCoroutine = null;
        }
        OnAutoSpinChanged?.Invoke(false);
    }

    public void ToggleAutoSpin()
    {
        if (IsAutoSpinning) StopAutoSpin();
        else StartAutoSpin();
    }

    private IEnumerator AutoSpinLoop()
    {
        while (IsAutoSpinning)
        {
            Spin();

            yield return new WaitForSeconds(autoSpinDelay);

            // Stop if the player can no longer afford the next spin
            if (PlayerEconomy.Instance == null || PlayerEconomy.Instance.Coins < betAmount)
            {
                StopAutoSpin();
                yield break;
            }
        }
    }

    public void Spin()
    {
        if (PlayerEconomy.Instance == null || !PlayerEconomy.Instance.SpendCoins(betAmount))
        {
            Debug.Log("Not enough coins to spin.");
            return;
        }

        // Spin reels
        Symbol[] results = new Symbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            results[i] = reels[i].Spin();

        // Evaluate 5-tier jackpot (also contributes bet to all pools internally)
        var (winTier, jackpotPrize) = ProgressiveJackpot.Instance != null
            ? ProgressiveJackpot.Instance.EvaluateSpin(betAmount)
            : ((ProgressiveJackpot.JackpotTier?)null, 0L);

        if (winTier.HasValue && jackpotPrize > 0)
        {
            jackpotPrize = LoyaltySystem.Instance?.ApplyTierBonus(jackpotPrize) ?? jackpotPrize;
            PlayerEconomy.Instance.AddCoins(jackpotPrize);
            LoyaltySystem.Instance?.RegisterSpin();
            OnSpinComplete?.Invoke(jackpotPrize, true);
            return;
        }

        long payout = CalculatePayout(results);
        float multiplier = PlayerEconomy.Instance.GetActiveMultiplier();
        payout = (long)(payout * multiplier);

        // Apply loyalty tier win bonus
        if (payout > 0)
            payout = LoyaltySystem.Instance?.ApplyTierBonus(payout) ?? payout;

        if (payout > 0)
            PlayerEconomy.Instance.AddCoins(payout);

        LoyaltySystem.Instance?.RegisterSpin();
        OnSpinComplete?.Invoke(payout, false);
    }

    private long CalculatePayout(Symbol[] results)
    {
        if (payoutTable == null || results.Length == 0) return 0;

        // Count matches for each symbol
        System.Collections.Generic.Dictionary<int, int> counts = new System.Collections.Generic.Dictionary<int, int>();
        foreach (var sym in results)
        {
            if (sym == null) continue;
            counts.TryGetValue(sym.symbolId, out int existing);
            counts[sym.symbolId] = existing + 1;
        }

        long best = 0;
        foreach (var kvp in counts)
        {
            float mult = payoutTable.GetMultiplier(kvp.Key, kvp.Value);
            long candidate = (long)(betAmount * mult);
            if (candidate > best) best = candidate;
        }

        // Apply active game RTP to enforce house advantage.
        // Default RTP = 0.70 (3/10 house edge).  Wins are weighted down so that
        // over many spins the house retains 30 cents of every coin wagered.
        float rtp = SlotGameLoader.Instance?.ActiveGame?.baseRTP ?? 0.70f;

        // Only scale winning spins — losing spins already contribute to edge.
        // We use RTP as an expected-value weight: a payout of X has rtp chance
        // of actually paying out and (1-rtp) chance of returning 0 on that spin.
        if (best > 0 && UnityEngine.Random.value > rtp)
            return 0;

        return best;
    }
}
