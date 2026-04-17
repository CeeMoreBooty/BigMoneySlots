using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Daily challenge system. Generates 3 fresh challenges each UTC day,
/// tracks per-challenge progress in PlayerPrefs, and awards coin+gem rewards.
/// </summary>
public class DailyChallenges : MonoBehaviour
{
    // ── Types ─────────────────────────────────────────────────────────────────

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

    // ── Static challenge pool ─────────────────────────────────────────────────

    private static readonly (ChallengeType Type, string Desc, long Target, long Coins, int Gems)[] ChallengeDefs =
    {
        (ChallengeType.SpinSlots,    "Spin the slots 50 times",           50L,          500_000L,   2),
        (ChallengeType.SpinSlots,    "Spin the slots 200 times",         200L,        2_000_000L,   5),
        (ChallengeType.WinCoins,     "Win 1 Million coins",        1_000_000L,        1_000_000L,   3),
        (ChallengeType.WinCoins,     "Win 100 Million coins",    100_000_000L,      100_000_000L,  10),
        (ChallengeType.WinCoins,     "Win 1 Billion coins",    1_000_000_000L,    1_000_000_000L,  20),
        (ChallengeType.WinCoins,     "Win 5 Billion coins",    5_000_000_000L,    5_000_000_000L,  50),
        (ChallengeType.WinBigWin,    "Get a Big Win",                      1L,        5_000_000L,   5),
        (ChallengeType.WinMegaWin,   "Get a Mega Win",                     1L,       25_000_000L,  10),
        (ChallengeType.WinEpicWin,   "Get an Epic Win",                    1L,      100_000_000L,  20),
        (ChallengeType.PlayGames,    "Play 10 games",                      10L,         250_000L,   1),
        (ChallengeType.CollectBonus, "Collect the daily bonus",             1L,        1_000_000L,  3),
        (ChallengeType.LevelUp,      "Level up once",                       1L,       10_000_000L,  5),
    };

    // ── State ─────────────────────────────────────────────────────────────────

    public List<Challenge> TodayChallenges { get; private set; } = new List<Challenge>();

    public static event Action<Challenge> OnChallengeCompleted;

    private const string DateKey           = "DC_Date";
    private const string ProgressKeyPrefix = "DC_Prog_";
    private const string DoneKeyPrefix     = "DC_Done_";
    private const string ClaimedKeyPrefix  = "DC_Claimed_";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        LoadOrReset();
    }

    private void LoadOrReset()
    {
        string today    = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string savedDay = PlayerPrefs.GetString(DateKey, "");

        if (savedDay != today)
        {
            PlayerPrefs.SetString(DateKey, today);
            GenerateChallenges(today);
            Save();
        }
        else
        {
            GenerateChallenges(today); // rebuild structure using deterministic seed
            LoadProgress();
        }
    }

    /// <summary>
    /// Deterministically selects 3 challenges for the given date using the date
    /// string as a random seed — same date always yields the same challenges.
    /// </summary>
    private void GenerateChallenges(string dateKey)
    {
        TodayChallenges.Clear();

        var seededRng = new System.Random(dateKey.GetHashCode());

        // Build a shuffled index list
        var indices = new List<int>();
        for (int i = 0; i < ChallengeDefs.Length; i++) indices.Add(i);
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = seededRng.Next(i + 1);
            int tmp = indices[i]; indices[i] = indices[j]; indices[j] = tmp;
        }

        var usedTypes = new HashSet<ChallengeType>();
        int slot = 0;
        foreach (int i in indices)
        {
            if (slot >= 3) break;
            var def = ChallengeDefs[i];
            if (usedTypes.Contains(def.Type)) continue;
            usedTypes.Add(def.Type);

            TodayChallenges.Add(new Challenge
            {
                id          = $"dc_{dateKey}_{slot}",
                type        = def.Type,
                description = def.Desc,
                target      = def.Target,
                progress    = 0,
                coinReward  = def.Coins,
                gemReward   = def.Gems,
            });
            slot++;
        }
    }

    private void LoadProgress()
    {
        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            c.progress     = long.TryParse(PlayerPrefs.GetString($"{ProgressKeyPrefix}{i}", "0"), out long p) ? p : 0;
            c.isComplete    = PlayerPrefs.GetInt($"{DoneKeyPrefix}{i}",    0) == 1;
            c.rewardClaimed = PlayerPrefs.GetInt($"{ClaimedKeyPrefix}{i}", 0) == 1;
        }
    }

    private void Save()
    {
        for (int i = 0; i < TodayChallenges.Count; i++)
        {
            var c = TodayChallenges[i];
            PlayerPrefs.SetString($"{ProgressKeyPrefix}{i}", c.progress.ToString());
            PlayerPrefs.SetInt($"{DoneKeyPrefix}{i}",        c.isComplete    ? 1 : 0);
            PlayerPrefs.SetInt($"{ClaimedKeyPrefix}{i}",     c.rewardClaimed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    // ── Progress tracking ─────────────────────────────────────────────────────

    /// <summary>Record progress for all active challenges of the given type.</summary>
    public void RecordProgress(ChallengeType type, long amount)
    {
        if (amount <= 0) return;
        bool changed = false;
        foreach (var c in TodayChallenges)
        {
            if (c.type != type || c.isComplete) continue;
            c.progress += amount;
            if (c.progress >= c.target)
            {
                c.progress   = c.target;
                c.isComplete = true;
                OnChallengeCompleted?.Invoke(c);
            }
            changed = true;
        }
        if (changed) Save();
    }

    // ── Convenience wrappers used by GameUIController ─────────────────────────

    /// <summary>Record a spin (called by GameUIController.HandleSpinStart).</summary>
    public void OnSpin()           => RecordProgress(ChallengeType.SpinSlots, 1L);

    /// <summary>Bet tracking — no challenge type is defined for bet amount; this is a no-op by design.</summary>
    public void OnBet(long amount) { /* intentionally empty: bet amount is not a tracked challenge metric */ }

    /// <summary>Record coin winnings (called by GameUIController.HandleWin).</summary>
    public void OnWin(long amount) => RecordProgress(ChallengeType.WinCoins, amount);

    /// <summary>Record a big-win event (called by GameUIController.HandleWin).</summary>
    public void OnBigWin()         => RecordProgress(ChallengeType.WinBigWin, 1L);

    /// <summary>Record a jackpot event — counts as an Epic Win for challenge purposes.</summary>
    public void OnJackpot()        => RecordProgress(ChallengeType.WinEpicWin, 1L);

    // ── Claim reward ──────────────────────────────────────────────────────────

    public bool ClaimReward(string challengeId)
    {
        var c = TodayChallenges.Find(x => x.id == challengeId);
        if (c == null || !c.isComplete || c.rewardClaimed) return false;

        PlayerEconomy.Instance?.AddCoins(c.coinReward);
        GemSystem.Instance?.AddGems(c.gemReward);
        c.rewardClaimed = true;
        Save();
        Debug.Log($"[DailyChallenges] Claimed '{c.description}': +{c.coinReward} coins, +{c.gemReward} gems");
        return true;
    }

    public bool IsClaimable(int index)
    {
        if (index < 0 || index >= TodayChallenges.Count) return false;
        var c = TodayChallenges[index];
        return c.isComplete && !c.rewardClaimed;
    }
}
