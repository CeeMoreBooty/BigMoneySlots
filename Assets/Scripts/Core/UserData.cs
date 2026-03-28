using System;
using UnityEngine;

/// <summary>
/// Manages persistent player data (coins, gems, level) with optional backend sync.
/// </summary>
public class UserData : MonoBehaviour
{
    private const string CoinsKey  = "UD_Coins";
    private const string GemsKey   = "UD_Gems";
    private const string LevelKey  = "UD_Level";
    private const string XPKey     = "UD_XP";

    public long Coins  { get; private set; }
    public int  Gems   { get; private set; }
    public int  Level  { get; private set; }
    public long XP     { get; private set; }

    // Starting values for new players
    private const long StartCoins = 10_000_000L;
    private const int  StartGems  = 5;

    public event Action OnDataChanged;

    public void Load()
    {
        long.TryParse(PlayerPrefs.GetString(CoinsKey, StartCoins.ToString()), out long coins);
        Coins = coins;
        Gems  = PlayerPrefs.GetInt(GemsKey, StartGems);
        Level = PlayerPrefs.GetInt(LevelKey, 1);
        long.TryParse(PlayerPrefs.GetString(XPKey, "0"), out long xp);
        XP = xp;
    }

    private void Save()
    {
        PlayerPrefs.SetString(CoinsKey, Coins.ToString());
        PlayerPrefs.SetInt(GemsKey, Gems);
        PlayerPrefs.SetInt(LevelKey, Level);
        PlayerPrefs.SetString(XPKey, XP.ToString());
        PlayerPrefs.Save();
        OnDataChanged?.Invoke();
    }

    public void AddCoins(long amount)
    {
        Coins += amount;
        Save();
    }

    public bool SpendCoins(long amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        Save();
        return true;
    }

    public void AddGems(int amount)
    {
        Gems += amount;
        Save();
    }

    public bool SpendGems(int amount)
    {
        if (Gems < amount) return false;
        Gems -= amount;
        Save();
        return true;
    }

    public void AddXP(long amount)
    {
        XP += amount;
        long xpForNextLevel = GetXPForLevel(Level + 1);
        while (XP >= xpForNextLevel)
        {
            XP -= xpForNextLevel;
            Level++;
            GameManager.Instance?.dailyChallenges?.RecordProgress(
                DailyChallenges.ChallengeType.LevelUp, 1L);
            xpForNextLevel = GetXPForLevel(Level + 1);
        }
        Save();
    }

    private long GetXPForLevel(int level)
    {
        return 1000L * level * level;
    }
}
