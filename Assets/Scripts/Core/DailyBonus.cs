using System;
using UnityEngine;

/// <summary>
/// Awards a daily login bonus. The bonus scales with consecutive login streak.
/// </summary>
public class DailyBonus : MonoBehaviour
{
    public static DailyBonus Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private const string LastClaimKey  = "DB_LastClaim";
    private const string StreakKey     = "DB_Streak";

    // Base bonus — multiplied by streak day (capped at day 7)
    private const long BaseBonus = 5_000_000L;
    private const int  MaxStreak = 7;

    public int  CurrentStreak  { get; private set; }
    public bool CanClaimToday  { get; private set; }

    public event Action<long, int> OnBonusAwarded; // (coins, streak)

    public void CheckAndAward()
    {
        string today     = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastClaim = PlayerPrefs.GetString(LastClaimKey, "");
        CurrentStreak    = PlayerPrefs.GetInt(StreakKey, 0);

        if (lastClaim == today)
        {
            CanClaimToday = false;
            return;
        }

        // If last claim was yesterday, increment streak; otherwise reset
        bool claimedYesterday = lastClaim ==
            DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        CurrentStreak = claimedYesterday
            ? Mathf.Min(CurrentStreak + 1, MaxStreak)
            : 1;

        CanClaimToday = true;
    }

    /// <summary>
    /// Call when the player taps "Collect" on the bonus screen.
    /// </summary>
    public void Collect()
    {
        if (!CanClaimToday) return;

        long bonus = BaseBonus * Mathf.Clamp(CurrentStreak, 1, MaxStreak);

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(LastClaimKey, today);
        PlayerPrefs.SetInt(StreakKey, CurrentStreak);
        PlayerPrefs.Save();

        CanClaimToday = false;

        GameManager.Instance?.OnDailyBonusCollected(bonus);
        OnBonusAwarded?.Invoke(bonus, CurrentStreak);
    }

    public long GetTodayBonus()
    {
        return BaseBonus * Mathf.Clamp(CurrentStreak, 1, MaxStreak);
    }
}
