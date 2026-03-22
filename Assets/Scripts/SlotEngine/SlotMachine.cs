using System;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [Header("Reels")]
    public Reel[] reels;

    [Header("Config")]
    public PayoutTable payoutTable;
    public long betAmount = 100;
    [Range(0f, 1f)] public float jackpotChance = 0.001f;

    public event Action<long, bool> OnSpinComplete; // (payout, isJackpot)

    public void Spin()
    {
        if (PlayerEconomy.Instance == null || !PlayerEconomy.Instance.SpendCoins(betAmount))
        {
            Debug.Log("Not enough coins to spin.");
            return;
        }

        ProgressiveJackpot.Instance?.Contribute(betAmount);

        Symbol[] results = new Symbol[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            results[i] = reels[i].Spin();

        // Check jackpot
        if (UnityEngine.Random.value <= jackpotChance)
        {
            long jackpotAmount = ProgressiveJackpot.Instance?.Payout() ?? 0;
            jackpotAmount = LoyaltySystem.Instance?.ApplyTierBonus(jackpotAmount) ?? jackpotAmount;
            PlayerEconomy.Instance.AddCoins(jackpotAmount);
            LoyaltySystem.Instance?.RegisterSpin();
            OnSpinComplete?.Invoke(jackpotAmount, true);
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
