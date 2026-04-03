using UnityEngine;

/// <summary>
/// Persistent game data – coins, level, stats.
/// Static class backed by PlayerPrefs.
/// </summary>
public static class GameData
{
    private const string COINS_KEY         = "Coins";
    private const string LEVEL_KEY         = "Level";
    private const string TOTAL_SPINS_KEY   = "TotalSpins";
    private const string TOTAL_WINS_KEY    = "TotalWins";
    private const string BIGGEST_WIN_KEY   = "BiggestWin";
    private const string LAST_BONUS_KEY    = "LastDailyBonus";
    private const string SETTINGS_MUSIC    = "SettingsMusic";
    private const string SETTINGS_SFX      = "SettingsSFX";
    private const string NOTIFICATIONS_KEY = "Notifications";

    public static long Coins
    {
        get => long.Parse(PlayerPrefs.GetString(COINS_KEY, "10000"));
        set => PlayerPrefs.SetString(COINS_KEY, value.ToString());
    }

    public static int Level
    {
        get => PlayerPrefs.GetInt(LEVEL_KEY, 1);
        set => PlayerPrefs.SetInt(LEVEL_KEY, value);
    }

    public static long TotalSpins
    {
        get => long.Parse(PlayerPrefs.GetString(TOTAL_SPINS_KEY, "0"));
        set => PlayerPrefs.SetString(TOTAL_SPINS_KEY, value.ToString());
    }

    public static long TotalWins
    {
        get => long.Parse(PlayerPrefs.GetString(TOTAL_WINS_KEY, "0"));
        set => PlayerPrefs.SetString(TOTAL_WINS_KEY, value.ToString());
    }

    public static long BiggestWin
    {
        get => long.Parse(PlayerPrefs.GetString(BIGGEST_WIN_KEY, "0"));
        set
        {
            if (value > BiggestWin)
                PlayerPrefs.SetString(BIGGEST_WIN_KEY, value.ToString());
        }
    }

    public static bool MusicEnabled
    {
        get => PlayerPrefs.GetInt(SETTINGS_MUSIC, 1) == 1;
        set => PlayerPrefs.SetInt(SETTINGS_MUSIC, value ? 1 : 0);
    }

    public static bool SFXEnabled
    {
        get => PlayerPrefs.GetInt(SETTINGS_SFX, 1) == 1;
        set => PlayerPrefs.SetInt(SETTINGS_SFX, value ? 1 : 0);
    }

    public static bool NotificationsEnabled
    {
        get => PlayerPrefs.GetInt(NOTIFICATIONS_KEY, 1) == 1;
        set => PlayerPrefs.SetInt(NOTIFICATIONS_KEY, value ? 1 : 0);
    }

    public static string LastDailyBonusDate
    {
        get => PlayerPrefs.GetString(LAST_BONUS_KEY, string.Empty);
        set => PlayerPrefs.SetString(LAST_BONUS_KEY, value);
    }

    public static bool CanClaimDailyBonus =>
        LastDailyBonusDate != System.DateTime.UtcNow.ToString("yyyy-MM-dd");

    public static void AddCoins(long amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        if (Coins > 999_999_999_999L) Coins = 999_999_999_999L;
    }

    public static bool SpendCoins(long amount)
    {
        if (amount <= 0 || Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    public static void Save() => PlayerPrefs.Save();
}
