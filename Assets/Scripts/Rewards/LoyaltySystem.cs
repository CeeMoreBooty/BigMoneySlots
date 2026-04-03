using System;
using UnityEngine;

/// <summary>
/// Prairie Guardians Loyalty System — massive advantage rewards for continued play.
/// Tracks total spins, session time, and tier progression to keep players engaged.
/// House advantage is 3/10 (30% edge); loyalty rewards recycle a portion back to players
/// as retention incentive while net profit remains strongly in the developer's favour.
/// </summary>
public class LoyaltySystem : MonoBehaviour
{
    public static LoyaltySystem Instance { get; private set; }

    // ── Tier definitions ─────────────────────────────────────────────────────
    public enum LoyaltyTier
    {
        TrailWanderer   = 0,   //     0 –   999 lifetime spins
        PrairieKeeper   = 1,   //  1000 –  4999
        GuardianScout   = 2,   //  5000 – 14999
        SpiritWarrior   = 3,   // 15000 – 49999
        GreatPlainsChief = 4,  // 50000+
    }

    private static readonly int[] TierThresholds = { 0, 1000, 5000, 15000, 50000 };

    // ── Spin milestones → coin dump ──────────────────────────────────────────
    private static readonly (int spins, long coins, int freeSpins, int superSpins)[] SpinMilestones =
    {
        (100,      5_000_000L,   5,  0),
        (500,     25_000_000L,  15,  1),
        (1_000,  100_000_000L,  30,  3),
        (2_500,  300_000_000L,  50,  5),
        (5_000,  750_000_000L, 100, 10),
        (10_000, 2_000_000_000L, 200, 25),
        (25_000, 6_000_000_000L, 500, 50),
        (50_000, 15_000_000_000L, 1000, 100),
    };

    // ── Session play reward (every 30 min of continuous play) ────────────────
    [Header("Session Reward — every 30 min of active play")]
    public long sessionCoinReward      = 50_000_000;
    public int  sessionFreeSpinReward  = 10;

    // ── Comeback bonus (returning after 24 h+ away) ──────────────────────────
    [Header("Comeback Bonus")]
    public long comebackCoins          = 200_000_000;
    public int  comebackFreeSpins      = 20;
    public int  comebackSuperSpins     = 2;

    // ── Tier multiplier on all coin wins ─────────────────────────────────────
    private static readonly float[] TierWinMultiplier = { 1f, 1.10f, 1.25f, 1.50f, 2.00f };

    // ── Tier daily bonus dumps ───────────────────────────────────────────────
    private static readonly long[] TierDailyBonus =
    {
        0L,
        10_000_000L,
        50_000_000L,
        200_000_000L,
        1_000_000_000L,
    };

    // ── PlayerPrefs keys ─────────────────────────────────────────────────────
    private const string KeyLifetimeSpins    = "loyalty_lifetime_spins";
    private const string KeyLastSessionTime  = "loyalty_last_session";
    private const string KeySessionStart     = "loyalty_session_start";
    private const string KeyLastDailyBonus   = "loyalty_daily_bonus_last";
    private const string KeyLastActiveTime   = "loyalty_last_active";
    private const string KeyMilestoneIndex   = "loyalty_milestone_idx";
    private const string KeyComebackGiven    = "loyalty_comeback_session";

    // ── Runtime state ────────────────────────────────────────────────────────
    public int           LifetimeSpins  { get; private set; }
    public LoyaltyTier   CurrentTier    { get; private set; }
    public float         WinMultiplier  => TierWinMultiplier[(int)CurrentTier];

    private double _sessionStartTime;
    private double _lastSessionRewardTime;
    private int    _nextMilestoneIndex;

    public static event Action<LoyaltyTier>  OnTierUp;
    public static event Action<long, int, int> OnMilestoneReached;  // coins, freeSpins, superSpins
    public static event Action<long, int>    OnSessionReward;       // coins, freeSpins
    public static event Action               OnComebackBonus;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        CheckComebackBonus();
        _sessionStartTime      = GetUtcNow();
        _lastSessionRewardTime = GetUtcNow();
    }

    private void Update()
    {
        CheckSessionReward();
        SaveLastActiveTime();
    }

    // ── Called by SlotMachine after every spin ────────────────────────────────
    public void RegisterSpin()
    {
        LifetimeSpins++;
        Save();
        CheckMilestones();
        CheckTierUp();
        CheckDailyTierBonus();
    }

    // ── Apply tier multiplier on top of slot win ──────────────────────────────
    public long ApplyTierBonus(long baseWin) => (long)(baseWin * WinMultiplier);

    // ── Internal checks ──────────────────────────────────────────────────────
    private void CheckMilestones()
    {
        while (_nextMilestoneIndex < SpinMilestones.Length &&
               LifetimeSpins >= SpinMilestones[_nextMilestoneIndex].spins)
        {
            var m = SpinMilestones[_nextMilestoneIndex];
            PlayerEconomy.Instance?.AddCoins(m.coins);
            PlayerEconomy.Instance?.AddFreeSpins(m.freeSpins);
            if (m.superSpins > 0) PlayerEconomy.Instance?.AddSuperSpins(m.superSpins);
            OnMilestoneReached?.Invoke(m.coins, m.freeSpins, m.superSpins);
            _nextMilestoneIndex++;
            PlayerPrefs.SetInt(KeyMilestoneIndex, _nextMilestoneIndex);
            Debug.Log($"[LoyaltySystem] Milestone reached at {LifetimeSpins} spins! +" +
                      $"{m.coins:N0} coins, +{m.freeSpins} free spins, +{m.superSpins} super spins");
        }
    }

    private void CheckTierUp()
    {
        LoyaltyTier newTier = LoyaltyTier.TrailWanderer;
        for (int i = TierThresholds.Length - 1; i >= 0; i--)
        {
            if (LifetimeSpins >= TierThresholds[i]) { newTier = (LoyaltyTier)i; break; }
        }

        if (newTier > CurrentTier)
        {
            CurrentTier = newTier;
            Save();
            OnTierUp?.Invoke(CurrentTier);
            Debug.Log($"[LoyaltySystem] Tier up! Now: {CurrentTier}  WinMultiplier: {WinMultiplier}×");
        }
    }

    private void CheckDailyTierBonus()
    {
        if (CurrentTier == LoyaltyTier.TrailWanderer) return;

        double last = double.TryParse(PlayerPrefs.GetString(KeyLastDailyBonus, "0"), out double v) ? v : 0;
        if (GetUtcNow() - last < 86400) return;

        long bonus = TierDailyBonus[(int)CurrentTier];
        PlayerEconomy.Instance?.AddCoins(bonus);
        PlayerPrefs.SetString(KeyLastDailyBonus, GetUtcNow().ToString());
        PlayerPrefs.Save();
        Debug.Log($"[LoyaltySystem] Daily tier bonus: +{bonus:N0} coins ({CurrentTier})");
    }

    private void CheckSessionReward()
    {
        if (GetUtcNow() - _lastSessionRewardTime < 1800) return; // 30 min
        _lastSessionRewardTime = GetUtcNow();
        PlayerEconomy.Instance?.AddCoins(sessionCoinReward);
        PlayerEconomy.Instance?.AddFreeSpins(sessionFreeSpinReward);
        OnSessionReward?.Invoke(sessionCoinReward, sessionFreeSpinReward);
        Debug.Log($"[LoyaltySystem] Session reward: +{sessionCoinReward:N0} coins, +{sessionFreeSpinReward} free spins");
    }

    private void CheckComebackBonus()
    {
        double lastActive = double.TryParse(PlayerPrefs.GetString(KeyLastActiveTime, "0"), out double v) ? v : 0;
        string todayKey   = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Only give comeback bonus if away for 24+ hours AND not already given today
        if (lastActive > 0 && GetUtcNow() - lastActive >= 86400 &&
            PlayerPrefs.GetString(KeyComebackGiven, "") != todayKey)
        {
            PlayerEconomy.Instance?.AddCoins(comebackCoins);
            PlayerEconomy.Instance?.AddFreeSpins(comebackFreeSpins);
            PlayerEconomy.Instance?.AddSuperSpins(comebackSuperSpins);
            PlayerPrefs.SetString(KeyComebackGiven, todayKey);
            PlayerPrefs.Save();
            OnComebackBonus?.Invoke();
            Debug.Log($"[LoyaltySystem] Comeback bonus granted!");
        }
    }

    private void SaveLastActiveTime()
    {
        // Update every 60 s to avoid excessive writes
        double last = double.TryParse(PlayerPrefs.GetString(KeyLastActiveTime, "0"), out double v) ? v : 0;
        if (GetUtcNow() - last > 60)
        {
            PlayerPrefs.SetString(KeyLastActiveTime, GetUtcNow().ToString());
            PlayerPrefs.Save();
        }
    }

    // ── Persistence ──────────────────────────────────────────────────────────
    private void Save()
    {
        PlayerPrefs.SetInt(KeyLifetimeSpins, LifetimeSpins);
        PlayerPrefs.SetInt("loyalty_tier", (int)CurrentTier);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        LifetimeSpins       = PlayerPrefs.GetInt(KeyLifetimeSpins, 0);
        CurrentTier         = (LoyaltyTier)PlayerPrefs.GetInt("loyalty_tier", 0);
        _nextMilestoneIndex = PlayerPrefs.GetInt(KeyMilestoneIndex, 0);
    }

    private static double GetUtcNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

    // ── Public helpers ───────────────────────────────────────────────────────
    public int   SpinsToNextTier()
    {
        int next = (int)CurrentTier + 1;
        if (next >= TierThresholds.Length) return 0;
        return TierThresholds[next] - LifetimeSpins;
    }

    public int   NextMilestoneSpins()
    {
        if (_nextMilestoneIndex >= SpinMilestones.Length) return 0;
        return SpinMilestones[_nextMilestoneIndex].spins;
    }

    public long  NextMilestoneCoins()
    {
        if (_nextMilestoneIndex >= SpinMilestones.Length) return 0;
        return SpinMilestones[_nextMilestoneIndex].coins;
    }
}
