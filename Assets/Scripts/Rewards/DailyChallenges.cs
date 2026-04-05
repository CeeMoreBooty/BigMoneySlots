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
        LevelUp,
        SpinCount,
        UseFreeSpin,
        BigWinMultiplier,
        PlayMultipleGames,
        PlayTournament
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

    public event Action<Challenge> OnChallengeCompleted;

    private const string DateKey = "DC_Date";
    private List<ChallengeTemplate> challengePool;
    private System.Random rng = new System.Random();
    private string dateKey;

    private void Awake()
    {
        BuildChallengePool();
        LoadOrReset();
    }

    private void BuildChallengePool()
    {
        challengePool = new List<ChallengeTemplate>
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
    }

    // ── Public API for DailyChallengesUI ─────────────────────────────────────

    public int ChallengeCount => TodayChallenges.Count;

    public (long current, long target) GetProgress(int index)
    {
        if (index < 0 || index >= TodayChallenges.Count) return (0, 1);
        var c = TodayChallenges[index];
        return (c.progress, c.target);
    }

    public string GetDescription(int index)
    {
        if (index < 0 || index >= TodayChallenges.Count) return "";
        return TodayChallenges[index].description;
    }

    public (long coins, int gems) ClaimChallenge(int index)
    {
        if (index < 0 || index >= TodayChallenges.Count) return (0, 0);
        var c = TodayChallenges[index];
        if (!c.isComplete || c.rewardClaimed) return (0, 0);

        c.rewardClaimed = true;
        Save();
        Debug.Log($"[DailyChallenges] Claimed: {c.description} → +{c.coinReward:N0} coins, +{c.gemReward} gems");
        return (c.coinReward, c.gemReward);
    }

    /// <summary>Returns true if the challenge is complete and not yet claimed.</summary>
    public bool IsClaimable(int index)
    {
        if (index < 0 || index >= TodayChallenges.Count) return false;
        var c = TodayChallenges[index];
        return c.isComplete && !c.rewardClaimed;
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
        var key    = "dc_games_today";
        var played = PlayerPrefs.GetString(key, "");
        if (!played.Contains(gameId))
        {
            PlayerPrefs.SetString(key, played + gameId + ",");
            PlayerPrefs.Save();
            RecordProgress(ChallengeType.PlayMultipleGames, 1);
        }
    }

    public void RecordTournamentEntry()
    {
        RefreshIfNewDay();
        RecordProgress(ChallengeType.PlayTournament, 1);
    }

    // ── Claim reward (by id) ─────────────────────────────────────────────────

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
                c.progress   = c.target;
                c.isComplete = true;
                OnChallengeCompleted?.Invoke(c);
                anyCompleted = true;
            }
        }
        if (anyCompleted) Save();
    }

    private void LoadOrReset()
    {
        dateKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string saved = PlayerPrefs.GetString(DateKey, "");

        if (saved == dateKey)
            Load();
        else
        {
            GenerateNewChallenges();
            PlayerPrefs.SetString(DateKey, dateKey);
            PlayerPrefs.SetString("dc_games_today", "");
            Save();
        }
    }

    private void RefreshIfNewDay()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (today == dateKey) return;

        dateKey = today;
        GenerateNewChallenges();
        PlayerPrefs.SetString(DateKey, dateKey);
        PlayerPrefs.SetString("dc_games_today", "");
        Save();
    }

    private void GenerateNewChallenges()
    {
        TodayChallenges.Clear();

        if (challengePool == null || challengePool.Count == 0) return;

        var chosen    = new List<int>();
        var usedTypes = new HashSet<ChallengeType>();
        int attempts  = 0;
        while (chosen.Count < 3 && attempts < 100)
        {
            int idx = rng.Next(challengePool.Count);
            attempts++;
            if (chosen.Contains(idx)) continue;
            if (usedTypes.Contains(challengePool[idx].Type)) continue;
            chosen.Add(idx);
            usedTypes.Add(challengePool[idx].Type);
        }

        for (int i = 0; i < chosen.Count; i++)
        {
            var p = challengePool[chosen[i]];
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

    private void Save()
    {
        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            PlayerPrefs.SetString($"dc_{i}_prog",    c.progress.ToString());
            PlayerPrefs.SetInt($"dc_{i}_done",    c.isComplete    ? 1 : 0);
            PlayerPrefs.SetInt($"dc_{i}_claimed", c.rewardClaimed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void Load()
    {
        GenerateNewChallenges();

        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            long prog;
            if (!long.TryParse(PlayerPrefs.GetString($"dc_{i}_prog", "0"), out prog))
                prog = 0;
            c.progress      = prog;
            c.isComplete    = PlayerPrefs.GetInt($"dc_{i}_done",    0) == 1;
            c.rewardClaimed = PlayerPrefs.GetInt($"dc_{i}_claimed", 0) == 1;
        }
    }
}
