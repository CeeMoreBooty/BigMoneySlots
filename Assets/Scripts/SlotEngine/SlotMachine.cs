using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Slot machine core — drives reels, bet management, payout calculation,
/// and auto-spin.  Calls FraudPrevention, DailyChallenges, and Analytics
/// on every spin so all systems stay in sync without extra wiring.
/// </summary>
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

    public event Action<long, bool> OnSpinComplete;    // (payout, isJackpot)
    public event Action<bool>       OnAutoSpinChanged; // (isRunning)

    public bool IsAutoSpinning { get; private set; }
    public bool IsSpinning     { get; private set; }   // true while reels resolving

    private Coroutine _autoSpinCoroutine;

    // ── Auto-spin ─────────────────────────────────────────────────────────────
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
        if (_autoSpinCoroutine != null) { StopCoroutine(_autoSpinCoroutine); _autoSpinCoroutine = null; }
        OnAutoSpinChanged?.Invoke(false);
    }

    public void ToggleAutoSpin()
    {
        if (IsAutoSpinning) StopAutoSpin(); else StartAutoSpin();
    }

    private IEnumerator AutoSpinLoop()
    {
        while (IsAutoSpinning)
        {
            Spin();
            yield return new WaitForSeconds(autoSpinDelay);

            if (PlayerEconomy.Instance == null || PlayerEconomy.Instance.Coins < betAmount)
            {
                StopAutoSpin();
                yield break;
            }
        }
    }

    // ── Spin ──────────────────────────────────────────────────────────────────
    /// <summary>
    /// Execute one spin.  Checks fraud rate limit, deducts bet, evaluates
    /// jackpot and symbol payouts, then notifies every subsystem.
    /// </summary>
    public void Spin()
    {
        // ── Fraud guard ────────────────────────────────────────────────────────
        if (FraudPrevention.Instance != null && !FraudPrevention.Instance.AllowSpin())
        {
            Debug.LogWarning("[SlotMachine] Spin blocked by FraudPrevention.");
            return;
        }

        // ── Balance check ──────────────────────────────────────────────────────
        if (PlayerEconomy.Instance == null || !PlayerEconomy.Instance.SpendCoins(betAmount))
        {
            Debug.Log("[SlotMachine] Not enough coins to spin.");
            return;
        }

        // ── Spin reels ─────────────────────────────────────────────────────────
        Symbol[] results = new Symbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            results[i] = reels[i].Spin();

        // ── Notify DailyChallenges: spin occurred ──────────────────────────────
        DailyChallenges.Instance?.RecordSpin();

        // ── Progressive jackpot evaluation ────────────────────────────────────
        var (winTier, jackpotPrize) = ProgressiveJackpot.Instance != null
            ? ProgressiveJackpot.Instance.EvaluateSpin(betAmount)
            : ((ProgressiveJackpot.JackpotTier?)null, 0L);

        if (winTier.HasValue && jackpotPrize > 0)
        {
            jackpotPrize = LoyaltySystem.Instance?.ApplyTierBonus(jackpotPrize) ?? jackpotPrize;
            PlayerEconomy.Instance.AddCoins(jackpotPrize);
            LoyaltySystem.Instance?.RegisterSpin();

            // Notify subsystems
            DailyChallenges.Instance?.RecordWin(jackpotPrize, betAmount);
            AnalyticsManager.Instance?.TrackSpin(betAmount, jackpotPrize, isJackpot: true);
            FraudPrevention.Instance?.ValidateWin(betAmount, jackpotPrize);
            FraudPrevention.Instance?.ValidateBalance();

            OnSpinComplete?.Invoke(jackpotPrize, true);
            return;
        }

        // ── Symbol payout ──────────────────────────────────────────────────────
        long payout = CalculatePayout(results);
        float multiplier = PlayerEconomy.Instance.GetActiveMultiplier();
        payout = (long)(payout * multiplier);

        if (payout > 0)
        {
            payout = LoyaltySystem.Instance?.ApplyTierBonus(payout) ?? payout;
            PlayerEconomy.Instance.AddCoins(payout);

            // DailyChallenges: record win amount and check for big-win multiplier
            DailyChallenges.Instance?.RecordWin(payout, betAmount);
        }

        LoyaltySystem.Instance?.RegisterSpin();

        // Notify subsystems
        AnalyticsManager.Instance?.TrackSpin(betAmount, payout, isJackpot: false);
        if (payout > 0) FraudPrevention.Instance?.ValidateWin(betAmount, payout);
        FraudPrevention.Instance?.ValidateBalance();

        OnSpinComplete?.Invoke(payout, false);
    }

    // ── Free-spin variant (does not deduct coins) ─────────────────────────────
    public void SpinFree()
    {
        if (PlayerEconomy.Instance == null || !PlayerEconomy.Instance.UseFreeSpins(1))
        {
            Debug.Log("[SlotMachine] No free spins remaining.");
            return;
        }

        // Same fraud / reel logic; DailyChallenges tracks free-spin use
        if (FraudPrevention.Instance != null && !FraudPrevention.Instance.AllowSpin())
            return;

        Symbol[] results = new Symbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            results[i] = reels[i].Spin();

        DailyChallenges.Instance?.RecordSpin();
        DailyChallenges.Instance?.RecordFreeSpin();

        long payout = CalculatePayout(results);
        float multiplier = PlayerEconomy.Instance.GetActiveMultiplier();
        payout = (long)(payout * multiplier);

        if (payout > 0)
        {
            payout = LoyaltySystem.Instance?.ApplyTierBonus(payout) ?? payout;
            PlayerEconomy.Instance.AddCoins(payout);
            DailyChallenges.Instance?.RecordWin(payout, betAmount);
        }

        LoyaltySystem.Instance?.RegisterSpin();
        AnalyticsManager.Instance?.TrackSpin(0, payout, isJackpot: false);
        OnSpinComplete?.Invoke(payout, false);
    }

    // ── Payout calculation ────────────────────────────────────────────────────
    private long CalculatePayout(Symbol[] results)
    {
        if (payoutTable == null || results.Length == 0) return 0;

        var counts = new System.Collections.Generic.Dictionary<int, int>();
        foreach (var sym in results)
        {
            if (sym == null) continue;
            counts.TryGetValue(sym.symbolId, out int existing);
            counts[sym.symbolId] = existing + 1;
        }

        long best = 0;
        foreach (var kvp in counts)
        {
            float mult      = payoutTable.GetMultiplier(kvp.Key, kvp.Value);
            long  candidate = (long)(betAmount * mult);
            if (candidate > best) best = candidate;
        }

        // Apply game RTP to enforce house edge.
        float rtp = SlotGameLoader.Instance?.ActiveGame?.baseRTP ?? 0.70f;
        if (best > 0 && UnityEngine.Random.value > rtp) return 0;

        return best;
    }
}
