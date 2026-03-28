using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("Hourly Reward")]
    public long hourlyCoins = 500_000;

    [Header("Daily Streak")]
    public long[] dailyStreakCoins = { 1_000_000, 2_000_000, 3_000_000, 5_000_000, 10_000_000, 20_000_000, 50_000_000 };

    [Header("New Player Pack")]
    public long newPlayerCoins = 20_000_000_000L;
    public int newPlayerFreeSpins = 50;
    public int newPlayerSuperSpins = 5;

    private const string KeyHourlyLast   = "reward_hourly_last";
    private const string KeyDailyLast    = "reward_daily_last";
    private const string KeyDailyStreak  = "reward_daily_streak";
    private const string KeyNewPlayer    = "reward_new_player_claimed";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GrantNewPlayerPackIfNeeded();
    }

    // --- New Player ---
    private void GrantNewPlayerPackIfNeeded()
    {
        if (PlayerPrefs.GetInt(KeyNewPlayer, 0) == 1) return;
        PlayerEconomy.Instance?.AddCoins(newPlayerCoins);
        PlayerEconomy.Instance?.AddFreeSpins(newPlayerFreeSpins);
        PlayerEconomy.Instance?.AddSuperSpins(newPlayerSuperSpins);
        PlayerPrefs.SetInt(KeyNewPlayer, 1);
        PlayerPrefs.Save();
        Debug.Log("New player pack granted!");
    }

    // --- Hourly ---
    public bool IsHourlyReady()
    {
        double last = double.TryParse(PlayerPrefs.GetString(KeyHourlyLast, "0"), out double v) ? v : 0;
        return GetUtcNow() - last >= 3600;
    }

    public bool ClaimHourlyReward()
    {
        if (!IsHourlyReady()) return false;
        PlayerEconomy.Instance?.AddCoins(hourlyCoins);
        PlayerPrefs.SetString(KeyHourlyLast, GetUtcNow().ToString());
        PlayerPrefs.Save();
        return true;
    }

    public double HourlySecondsRemaining()
    {
        double last = double.TryParse(PlayerPrefs.GetString(KeyHourlyLast, "0"), out double v) ? v : 0;
        double remaining = 3600 - (GetUtcNow() - last);
        return remaining < 0 ? 0 : remaining;
    }

    // --- Daily Streak ---
    public bool IsDailyReady()
    {
        double last = double.TryParse(PlayerPrefs.GetString(KeyDailyLast, "0"), out double v) ? v : 0;
        return GetUtcNow() - last >= 86400;
    }

    public bool ClaimDailyReward()
    {
        if (!IsDailyReady()) return false;

        double last = double.TryParse(PlayerPrefs.GetString(KeyDailyLast, "0"), out double v) ? v : 0;
        int streak = PlayerPrefs.GetInt(KeyDailyStreak, 0);

        // Reset streak if more than 48 h have passed
        if (GetUtcNow() - last > 172800) streak = 0;

        int day = streak % dailyStreakCoins.Length;
        PlayerEconomy.Instance?.AddCoins(dailyStreakCoins[day]);

        PlayerPrefs.SetString(KeyDailyLast, GetUtcNow().ToString());
        PlayerPrefs.SetInt(KeyDailyStreak, streak + 1);
        PlayerPrefs.Save();
        return true;
    }

    public double DailySecondsRemaining()
    {
        double last = double.TryParse(PlayerPrefs.GetString(KeyDailyLast, "0"), out double v) ? v : 0;
        double remaining = 86400 - (GetUtcNow() - last);
        return remaining < 0 ? 0 : remaining;
    }

    public int GetCurrentStreak() => PlayerPrefs.GetInt(KeyDailyStreak, 0);

    private static double GetUtcNow() =>
        (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
}
