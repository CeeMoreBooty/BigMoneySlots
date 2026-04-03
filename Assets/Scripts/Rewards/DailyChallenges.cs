using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Daily Challenges — 3 unique challenges generated each day at midnight UTC.
/// Completing challenges awards gems + coins.
///
/// Wiring (automatic, no Inspector setup needed):
///   • SlotMachine.Spin()        → RecordSpin()  / RecordWin()  via direct calls
///   • SlotMachine.SpinFree()    → RecordSpin()  / RecordFreeSpin()
///   • ProgressiveJackpot.OnJackpotWon   → RecordProgress(PlayTournament)
///   • SlotGameLoader.OnGameChanged      → RecordGamePlayed(gameId)
/// </summary>
public class DailyChallenges : MonoBehaviour
{
    public static DailyChallenges Instance { get; private set; }

    public enum ChallengeType
    {
        SpinCount,          // Spin X times today
        BigWinMultiplier,   // Land a win ≥ 10× your bet
        UseFreeSpin,        // Use X free spins
        PlayTournament,     // Win a jackpot (any tier)
        WinCoins,           // Win X coins total today
        PlayMultipleGames,  // Play X different slot games
    }

    [Serializable]
    public class Challenge
    {
        public string        id;
        public ChallengeType type;
        public string        description;
        public long          target;      // long so 5B-coin goals fit
        public long          progress;
        public bool          isComplete;
        public bool          rewardClaimed;
        public long          coinReward;
        public int           gemReward;
    }

    public List<Challenge> TodayChallenges { get; private set; } = new List<Challenge>();

    public static event Action              OnChallengesRefreshed;
    public static event Action<Challenge>   OnChallengeCompleted;

    private string _lastChallengeDay = "";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshIfNewDay();
    }

    private void Start()
    {
        // Jackpot win → tournament challenge
        ProgressiveJackpot.OnJackpotWon += OnJackpotWon;

        // Game change → PlayMultipleGames challenge
        SlotGameLoader.OnGameChanged += OnGameChanged;
    }

    private void OnDestroy()
    {
        ProgressiveJackpot.OnJackpotWon -= OnJackpotWon;
        SlotGameLoader.OnGameChanged    -= OnGameChanged;
    }

    // ── Event callbacks ───────────────────────────────────────────────────────
    private void OnJackpotWon(ProgressiveJackpot.JackpotTier tier, long amount, string label)
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.PlayTournament, 1);
    }

    private void OnGameChanged(SlotGameConfig config)
    {
        if (config != null) RecordGamePlayed(config.gameId);
    }

    // ── Public progress API (called by SlotMachine) ───────────────────────────

    /// <summary>Call after every spin (coin or free).</summary>
    public void RecordSpin()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.SpinCount, 1);
    }

    /// <summary>Call when a free spin is consumed.</summary>
    public void RecordFreeSpin()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.UseFreeSpin, 1);
    }

    /// <summary>Call after a winning spin with the payout and the bet used.</summary>
    public void RecordWin(long amount, long bet)
    {
        RefreshIfNewDay();
        // Accumulate coins toward WinCoins challenge (clamped to long)
        RecordProgress(ChallengeType.WinCoins, (long)Math.Min(amount, long.MaxValue));
        // A win of ≥ 10× the bet counts as a BigWin
        if (bet > 0 && amount >= bet * 10)
            RecordProgress(ChallengeType.BigWinMultiplier, 1);
    }

    /// <summary>Call when the player changes active slot games.</summary>
    public void RecordGamePlayed(string gameId)
    {
        RefreshIfNewDay();
        const string key = "dc_games_today";
        string played = PlayerPrefs.GetString(key, "");
        if (played.Contains(gameId)) return;
        PlayerPrefs.SetString(key, played + gameId + ",");
        PlayerPrefs.Save();
        RecordProgress(ChallengeType.PlayMultipleGames, 1);
    }

    /// <summary>Manually record a tournament entry (e.g. from lobby).</summary>
    public void RecordTournamentEntry()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.PlayTournament, 1);
    }

    // ── Reward claiming ───────────────────────────────────────────────────────
    public bool ClaimReward(string challengeId)
    {
        var c = TodayChallenges.Find(x => x.id == challengeId);
        if (c == null || !c.isComplete || c.rewardClaimed) return false;

        PlayerEconomy.Instance?.AddCoins(c.coinReward);
        GemSystem.Instance?.AddGems(c.gemReward);
        c.rewardClaimed = true;
        Save();
        Debug.Log($"[DailyChallenges] Claimed '{c.description}' → +{c.coinReward:N0} coins, +{c.gemReward} gems");
        return true;
    }

    // ── Internal logic ────────────────────────────────────────────────────────
    private void RecordProgress(ChallengeType type, long amount)
    {
        bool anyCompleted = false;
        foreach (var c in TodayChallenges)
        {
            if (c.type != type || c.isComplete) continue;
            c.progress += amount;
            if (c.progress >= c.target)
            {
                c.progress   = c.target;
                c.isComplete = true;
                OnChallengeCompleted?.Invoke(c);
                anyCompleted = true;
                Debug.Log($"[DailyChallenges] Challenge complete: {c.description}");
            }
        }
        if (anyCompleted) Save();
    }

    private void RefreshIfNewDay()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (_lastChallengeDay == today) return;
        _lastChallengeDay = today;
        GenerateChallenges(today);
    }

    private void GenerateChallenges(string dateKey)
    {
        var rng = new System.Random(dateKey.GetHashCode());

        TodayChallenges.Clear();
        PlayerPrefs.SetString("dc_games_today", "");

        // Pool — long targets so 5B-coin WinCoins goals work
        var pool = new List<(ChallengeType t, string desc, long target, long coins, int gems)>
        {
            (ChallengeType.SpinCount,        "Spin 50 times today",          50L,          100_000_000L,   5),
            (ChallengeType.SpinCount,        "Spin 150 times today",        150L,          350_000_000L,  15),
            (ChallengeType.SpinCount,        "Spin 300 times today",        300L,          800_000_000L,  30),
            (ChallengeType.BigWinMultiplier, "Land a 10× win",                1L,          200_000_000L,  10),
            (ChallengeType.BigWinMultiplier, "Land 3 wins of 10× or more",    3L,          600_000_000L,  25),
            (ChallengeType.UseFreeSpin,      "Use 10 free spins",            10L,          150_000_000L,   8),
            (ChallengeType.UseFreeSpin,      "Use 30 free spins",            30L,          400_000_000L,  20),
            (ChallengeType.PlayTournament,   "Hit any Jackpot",               1L,          250_000_000L,  12),
            (ChallengeType.WinCoins,         "Win 1B coins today",    1_000_000_000L,    300_000_000L,  15),
            (ChallengeType.WinCoins,         "Win 5B coins today",    5_000_000_000L,  1_000_000_000L,  50),
            (ChallengeType.PlayMultipleGames,"Play 3 different games",         3L,          500_000_000L,  20),
            (ChallengeType.PlayMultipleGames,"Play 5 different games",         5L,        1_200_000_000L,  40),
        };

        // Pick 3 challenges with distinct ChallengeType values
        var chosen   = new List<int>();
        var usedTypes= new HashSet<ChallengeType>();
        int attempts = 0;
        while (chosen.Count < 3 && attempts < 100)
        {
            int idx = rng.Next(pool.Count);
            attempts++;
            if (chosen.Contains(idx) || usedTypes.Contains(pool[idx].t)) continue;
            chosen.Add(idx);
            usedTypes.Add(pool[idx].t);
        }

        for (int i = 0; i < chosen.Count; i++)
        {
            var p = pool[chosen[i]];
            TodayChallenges.Add(new Challenge
            {
                id          = $"dc_{dateKey}_{i}",
                type        = p.t,
                description = p.desc,
                target      = p.target,
                coinReward  = p.coins,
                gemReward   = p.gems,
            });
        }

        LoadProgress(dateKey);

        PlayerPrefs.SetString("dc_last_day", dateKey);
        PlayerPrefs.Save();
        OnChallengesRefreshed?.Invoke();
        Debug.Log($"[DailyChallenges] Generated {TodayChallenges.Count} challenges for {dateKey}");
    }

    // ── Persistence ───────────────────────────────────────────────────────────
    private void Save()
    {
        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            PlayerPrefs.SetString($"dc_{i}_prog",    c.progress.ToString());
            PlayerPrefs.SetInt($"dc_{i}_done",       c.isComplete    ? 1 : 0);
            PlayerPrefs.SetInt($"dc_{i}_claimed",    c.rewardClaimed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadProgress(string dateKey)
    {
        string savedDay = PlayerPrefs.GetString("dc_last_day", "");
        if (savedDay != dateKey) return;  // new day → start fresh

        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            long.TryParse(PlayerPrefs.GetString($"dc_{i}_prog", "0"), out long prog);
            c.progress      = prog;
            c.isComplete    = PlayerPrefs.GetInt($"dc_{i}_done",    0) == 1;
            c.rewardClaimed = PlayerPrefs.GetInt($"dc_{i}_claimed", 0) == 1;
        }
    }
}
