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
            PlayerEconomy.Instance.AddCoins(jackpotAmount);
            OnSpinComplete?.Invoke(jackpotAmount, true);
            return;
        }

        long payout = CalculatePayout(results);
        float multiplier = PlayerEconomy.Instance.GetActiveMultiplier();
        payout = (long)(payout * multiplier);

        if (payout > 0)
            PlayerEconomy.Instance.AddCoins(payout);

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
        return best;
    }
}
