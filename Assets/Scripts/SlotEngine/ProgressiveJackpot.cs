using System;
using UnityEngine;

/// <summary>
/// Five-tier Progressive Jackpot system with dynamic win probability.
/// Tiers: Mini → Minor → Major → Grand → Mega (fully progressive).
/// Each tier's trigger chance INCREASES as its pool grows — the bigger
/// the pot, the more likely it fires, keeping players engaged.
/// </summary>
public class ProgressiveJackpot : MonoBehaviour
{
    public static ProgressiveJackpot Instance { get; private set; }

    public enum JackpotTier { Mini = 0, Minor = 1, Major = 2, Grand = 3, Mega = 4 }

    [Serializable]
    public class JackpotTierData
    {
        public JackpotTier tier;
        public string      label;
        public long        basePool;
        public long        currentPool;
        [Range(0f, 1f)]
        public float       baseChance;
        public float       contributionRate;
        public long        maxPoolBeforeSoft;
        [Range(0f, 1f)]
        public float       softCapChance;
    }

    [Header("Jackpot Tiers — edit base values in Inspector")]
    public JackpotTierData[] tiers = new JackpotTierData[]
    {
        // Mini  — fires often (~8-18%), small prize
        new JackpotTierData { tier = JackpotTier.Mini,  label = "MINI",  basePool = 50_000,      baseChance = 0.08f,    contributionRate = 0.004f, maxPoolBeforeSoft = 500_000,         softCapChance = 0.18f },
        // Minor — fires regularly (~2.5-7%)
        new JackpotTierData { tier = JackpotTier.Minor, label = "MINOR", basePool = 500_000,     baseChance = 0.025f,   contributionRate = 0.006f, maxPoolBeforeSoft = 5_000_000,       softCapChance = 0.07f  },
        // Major — occasional (~0.6-2.2%)
        new JackpotTierData { tier = JackpotTier.Major, label = "MAJOR", basePool = 5_000_000,   baseChance = 0.006f,   contributionRate = 0.008f, maxPoolBeforeSoft = 50_000_000,      softCapChance = 0.022f },
        // Grand — rare but exciting (~0.15-0.6%)
        new JackpotTierData { tier = JackpotTier.Grand, label = "GRAND", basePool = 50_000_000,  baseChance = 0.0015f,  contributionRate = 0.010f, maxPoolBeforeSoft = 500_000_000,     softCapChance = 0.006f },
        // Mega  — progressive, fires when pool is huge (~0.035-0.18%)
        new JackpotTierData { tier = JackpotTier.Mega,  label = "MEGA",  basePool = 500_000_000, baseChance = 0.00035f, contributionRate = 0.012f, maxPoolBeforeSoft = 10_000_000_000L, softCapChance = 0.0018f },
    };

    public static event Action<JackpotTier, long, string> OnJackpotWon;  // tier, amount, label
    public static event Action                            OnPoolsUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPools();
    }

    /// <summary>
    /// Evaluate one spin: contribute to pools, then check each tier highest-first.
    /// Returns the winning tier (or null) and prize amount.
    /// </summary>
    public (JackpotTier? tier, long amount) EvaluateSpin(long betAmount)
    {
        foreach (var t in tiers)
            t.currentPool = Math.Max(t.basePool, t.currentPool + (long)(betAmount * t.contributionRate));

        SavePools();
        OnPoolsUpdated?.Invoke();

        for (int i = tiers.Length - 1; i >= 0; i--)
        {
            if (SecureRandom.Value() < DynamicChance(tiers[i]))
            {
                long prize = tiers[i].currentPool;
                tiers[i].currentPool = tiers[i].basePool;
                SavePools();
                OnJackpotWon?.Invoke(tiers[i].tier, prize, tiers[i].label);
                return (tiers[i].tier, prize);
            }
        }

        return (null, 0);
    }

    /// <summary>
    /// Probability scales from baseChance → softCapChance as pool fills.
    /// A bigger jackpot pool = higher chance to hit = more excitement.
    /// </summary>
    public float DynamicChance(JackpotTierData t)
    {
        if (t.maxPoolBeforeSoft <= t.basePool) return t.baseChance;
        float fill = Mathf.Clamp01(
            (float)(t.currentPool - t.basePool) /
            (float)(t.maxPoolBeforeSoft - t.basePool));
        return Mathf.Lerp(t.baseChance, t.softCapChance, fill);
    }

    public long  GetPool(JackpotTier tier)  => tiers[(int)tier].currentPool;
    public float GetChance(JackpotTier tier) => DynamicChance(tiers[(int)tier]);

    /// <summary>Convenience property returning the Mega (top-tier) jackpot pool for simple UI display.</summary>
    public long CurrentJackpot => GetPool(JackpotTier.Mega);

    // ── Persistence ──────────────────────────────────────────────────────────
    private void SavePools()
    {
        foreach (var t in tiers)
            PlayerPrefs.SetString($"jp_{t.tier}", t.currentPool.ToString());
        PlayerPrefs.Save();
    }

    private void LoadPools()
    {
        foreach (var t in tiers)
        {
            long saved = long.TryParse(PlayerPrefs.GetString($"jp_{t.tier}", "0"), out long v) ? v : 0;
            t.currentPool = Math.Max(t.basePool, saved);
        }
    }
}
