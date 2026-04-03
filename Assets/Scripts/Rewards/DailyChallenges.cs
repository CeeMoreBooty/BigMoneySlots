using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyChallenges : MonoBehaviour
{
    public enum ChallengeType
    {
        SpinSlots,
        WinCoins,
        WinBigWin,
        WinMegaWin,
        WinEpicWin,
        PlayGames,
        CollectBonus,
        LevelUp
    }

    [Serializable]
    public class Challenge
    {
        public string        id;
        public ChallengeType type;
        public string        description;
        public long          target;
        public long          progress;
        public bool          isComplete;
        public bool          rewardClaimed;
        public long          coinReward;
        public int           gemReward;
    }

    private class ChallengeTemplate
    {
        public readonly ChallengeType Type;
        public readonly string Description;
        public readonly long Target;
        public readonly long Coins;
        public readonly int Gems;

        public ChallengeTemplate(ChallengeType type, string description, long target, long coins, int gems)
        {
            Type = type;
            Description = description;
            Target = target;
            Coins = coins;
            Gems = gems;
        }
    }

    public List<Challenge> TodayChallenges { get; private set; } = new List<Challenge>();

    private const string ProgressKeyPrefix = "DC_Progress_";
    private const string ClaimedKeyPrefix  = "DC_Claimed_";
    private const string DateKey           = "DC_Date";

    private void Awake()
    {
        BuildChallengeDefs();
        LoadOrReset();
    }

    private void BuildChallengeDefs()
    {
        challengeDefs = new List<(ChallengeType, string, long, long, int)>();

        challengeDefs.Add((ChallengeType.SpinSlots,    "Spin the slots 50 times",          50L,         500_000L,  2));
        challengeDefs.Add((ChallengeType.SpinSlots,    "Spin the slots 200 times",         200L,       2_000_000L,  5));
        challengeDefs.Add((ChallengeType.WinCoins,     "Win 1 Million coins",        1_000_000L,       1_000_000L,  3));
        challengeDefs.Add((ChallengeType.WinCoins,     "Win 100 Million coins",    100_000_000L,     100_000_000L, 10));
        challengeDefs.Add((ChallengeType.WinCoins,     "Win 1 Billion coins",    1_000_000_000L,   1_000_000_000L, 20));
        challengeDefs.Add((ChallengeType.WinCoins,     "Win 5 Billion coins",    5_000_000_000L,   5_000_000_000L, 50));
        challengeDefs.Add((ChallengeType.WinBigWin,    "Get a Big Win",                     1L,       5_000_000L,  5));
        challengeDefs.Add((ChallengeType.WinMegaWin,   "Get a Mega Win",                    1L,      25_000_000L, 10));
        challengeDefs.Add((ChallengeType.WinEpicWin,   "Get an Epic Win",                   1L,     100_000_000L, 20));
        challengeDefs.Add((ChallengeType.PlayGames,    "Play 10 games",                    10L,         250_000L,  1));
        challengeDefs.Add((ChallengeType.CollectBonus, "Collect the daily bonus",           1L,       1_000_000L,  3));
        challengeDefs.Add((ChallengeType.LevelUp,      "Level up once",                     1L,      10_000_000L,  5));
    }

    // ── Progress tracking ─────────────────────────────────────────────────────

    public void RecordSpin()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.SpinCount, 1);
    }

    public void RecordFreeSpin()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.UseFreeSpin, 1);
    }

    public void RecordWin(long amount, long bet)
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.WinCoins, amount);
        if (bet > 0 && amount >= bet * 10)
            RecordProgress(ChallengeType.BigWinMultiplier, 1);
    }

    public void RecordGamePlayed(string gameId)
    {
        RefreshIfNewDay();
        // Count unique games played today
        var key   = "dc_games_today";
        var played = PlayerPrefs.GetString(key, "");
        if (!played.Contains(gameId))
        {
            PlayerPrefs.SetString(key, played + gameId + ",");
            PlayerPrefs.Save();
            RecordProgress(ChallengeType.PlayMultipleGames, 1);
        }
    }

    public void RecordTournamentEntry() => RecordProgress(ChallengeType.PlayTournament, 1);

    // ── Claim reward ──────────────────────────────────────────────────────────

    public bool ClaimReward(string challengeId)
    {
        var c = TodayChallenges.Find(x => x.id == challengeId);
        if (c == null || !c.isComplete || c.rewardClaimed) return false;

        PlayerEconomy.Instance?.AddCoins(c.coinReward);
        GemSystem.Instance?.AddGems(c.gemReward);
        c.rewardClaimed = true;
        Save();
        Debug.Log($"[DailyChallenges] Claimed: {c.description} → +{c.coinReward:N0} coins, +{c.gemReward} gems");
        return true;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void RecordProgress(ChallengeType type, long amount)
    {
        bool anyCompleted = false;
        foreach (var c in TodayChallenges)
        {
            if (c.type != type || c.isComplete) continue;
            c.progress += amount;
            if (c.progress >= c.target)
            {
                c.progress  = c.target;
                c.isComplete = true;
                OnChallengeCompleted?.Invoke(c);
                anyCompleted = true;
            }
        }
        if (anyCompleted) Save();
    }

    private void RefreshIfNewDay()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string saved = PlayerPrefs.GetString(DateKey, "");

        progress = new long[challengeDefs.Count];
        claimed  = new bool[challengeDefs.Count];

        TodayChallenges.Clear();
        PlayerPrefs.SetString("dc_games_today", "");

        var pool = new List<ChallengeTemplate>
        {
            new ChallengeTemplate(ChallengeType.SpinCount,         "Spin 50 times today",              50,            100_000_000L,   5),
            new ChallengeTemplate(ChallengeType.SpinCount,         "Spin 150 times today",             150,           350_000_000L,  15),
            new ChallengeTemplate(ChallengeType.SpinCount,         "Spin 300 times today",             300,           800_000_000L,  30),
            new ChallengeTemplate(ChallengeType.BigWinMultiplier,  "Land a 10× win",                   1,             200_000_000L,  10),
            new ChallengeTemplate(ChallengeType.BigWinMultiplier,  "Land 3 wins of 10× or more",       3,             600_000_000L,  25),
            new ChallengeTemplate(ChallengeType.UseFreeSpin,       "Use 10 free spins",                10,            150_000_000L,   8),
            new ChallengeTemplate(ChallengeType.UseFreeSpin,       "Use 30 free spins",                30,            400_000_000L,  20),
            new ChallengeTemplate(ChallengeType.PlayTournament,    "Enter a tournament",               1,             250_000_000L,  12),
            new ChallengeTemplate(ChallengeType.WinCoins,          "Win 1B coins total today",         1_000_000_000, 300_000_000L,  15),
            new ChallengeTemplate(ChallengeType.WinCoins,          "Win 5B coins total today",         5_000_000_000L, 1_000_000_000L, 50),
            new ChallengeTemplate(ChallengeType.PlayMultipleGames, "Play 3 different slot games",      3,             500_000_000L,  20),
            new ChallengeTemplate(ChallengeType.PlayMultipleGames, "Play 5 different slot games",      5,             1_200_000_000L, 40),
        };

        // Pick 3 non-duplicate type challenges
        var chosen = new List<int>();
        var usedTypes = new HashSet<ChallengeType>();
        while (chosen.Count < 3 && chosen.Count < pool.Count)
        {
            int idx = rng.Next(pool.Count);
            if (chosen.Contains(idx)) continue;
            if (usedTypes.Contains(pool[idx].Type)) continue;
            chosen.Add(idx);
            usedTypes.Add(pool[idx].Type);
        }

        for (int i = 0; i < chosen.Count; i++)
        {
            var p = pool[chosen[i]];
            TodayChallenges.Add(new Challenge
            {
                id          = $"dc_{dateKey}_{i}",
                type        = p.Type,
                description = p.Description,
                target      = p.Target,
                coinReward  = p.Coins,
                gemReward   = p.Gems,
            });
        }
    }

    /// <summary>Record progress for all challenges of the given type.</summary>
    public void RecordProgress(ChallengeType type, long amount)
    {
        for (int i = 0; i < challengeDefs.Count; i++)
        {
            var c = TodayChallenges[i];
            PlayerPrefs.SetString($"dc_{i}_prog",    c.progress.ToString());
            PlayerPrefs.SetInt($"dc_{i}_done",    c.isComplete    ? 1 : 0);
            PlayerPrefs.SetInt($"dc_{i}_claimed", c.rewardClaimed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    /// <summary>Returns true if the challenge is complete and not yet claimed.</summary>
    public bool IsClaimable(int index)
    {
        string savedDay = PlayerPrefs.GetString("dc_last_day", "");
        if (savedDay != dateKey) return;  // different day → fresh challenges
        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            long progress;
            if (!long.TryParse(PlayerPrefs.GetString($"dc_{i}_prog", "0"), out progress))
                progress = 0;
            c.progress     = progress;
            c.isComplete    = PlayerPrefs.GetInt($"dc_{i}_done",    0) == 1;
            c.rewardClaimed = PlayerPrefs.GetInt($"dc_{i}_claimed", 0) == 1;
        }
    }
}
