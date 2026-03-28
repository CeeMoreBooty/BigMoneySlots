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

    // (type, desc, target, coins reward, gems reward)
    private List<(ChallengeType type, string desc, long target, long coins, int gems)> challengeDefs;

    private long[] progress;
    private bool[] claimed;

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

    private void LoadOrReset()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string saved = PlayerPrefs.GetString(DateKey, "");

        progress = new long[challengeDefs.Count];
        claimed  = new bool[challengeDefs.Count];

        if (saved != today)
        {
            // New day – clear progress
            for (int i = 0; i < challengeDefs.Count; i++)
            {
                PlayerPrefs.SetString(ProgressKeyPrefix + i, "0");
                PlayerPrefs.SetInt(ClaimedKeyPrefix + i, 0);
            }
            PlayerPrefs.SetString(DateKey, today);
            PlayerPrefs.Save();
        }
        else
        {
            for (int i = 0; i < challengeDefs.Count; i++)
            {
                long.TryParse(PlayerPrefs.GetString(ProgressKeyPrefix + i, "0"), out progress[i]);
                claimed[i] = PlayerPrefs.GetInt(ClaimedKeyPrefix + i, 0) == 1;
            }
        }
    }

    /// <summary>Record progress for all challenges of the given type.</summary>
    public void RecordProgress(ChallengeType type, long amount)
    {
        for (int i = 0; i < challengeDefs.Count; i++)
        {
            if (challengeDefs[i].type != type) continue;
            progress[i] += amount;
            if (progress[i] > challengeDefs[i].target)
                progress[i] = challengeDefs[i].target;
            PlayerPrefs.SetString(ProgressKeyPrefix + i, progress[i].ToString());
        }
        PlayerPrefs.Save();
    }

    /// <summary>Returns true if the challenge is complete and not yet claimed.</summary>
    public bool IsClaimable(int index)
    {
        if (index < 0 || index >= challengeDefs.Count) return false;
        return !claimed[index] && progress[index] >= challengeDefs[index].target;
    }

    /// <summary>Claim a completed challenge. Returns (coins, gems) reward or (0,0) if not claimable.</summary>
    public (long coins, int gems) ClaimChallenge(int index)
    {
        if (!IsClaimable(index)) return (0L, 0);

        claimed[index] = true;
        PlayerPrefs.SetInt(ClaimedKeyPrefix + index, 1);
        PlayerPrefs.Save();

        return (challengeDefs[index].coins, challengeDefs[index].gems);
    }

    /// <summary>Returns (progress, target) for a challenge.</summary>
    public (long current, long target) GetProgress(int index)
    {
        if (index < 0 || index >= challengeDefs.Count) return (0L, 0L);
        return (progress[index], challengeDefs[index].target);
    }

    /// <summary>Returns the number of defined challenges.</summary>
    public int ChallengeCount => challengeDefs?.Count ?? 0;

    /// <summary>Returns the description for a challenge.</summary>
    public string GetDescription(int index)
    {
        if (index < 0 || index >= challengeDefs.Count) return string.Empty;
        return challengeDefs[index].desc;
    }
}
