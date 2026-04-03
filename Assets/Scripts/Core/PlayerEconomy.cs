using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    // Currencies
    public long Coins { get; private set; }
    public int FreeSpins { get; private set; }
    public int SuperSpins { get; private set; }
    public int UltraSpins { get; private set; }

    // Cosmetics
    public string BadgeId { get; private set; }
    public string ReelSkinId { get; private set; }
    public string ProfileImageUrl { get; private set; }

    // Boost
    public float WinMultiplier { get; private set; } = 1f;
    private double _boostExpiryTime;

    private const long   StartingCoins   = 10_000;

    private const string KeyCoins       = "eco_coins";
    private const string KeyFreeSpins   = "eco_free_spins";
    private const string KeySuperSpins  = "eco_super_spins";
    private const string KeyUltraSpins  = "eco_ultra_spins";
    private const string KeyBadge       = "eco_badge";
    private const string KeyReelSkin    = "eco_reel_skin";
    private const string KeyProfileImg  = "eco_profile_img";
    private const string KeyMultiplier  = "eco_win_mult";
    private const string KeyBoostExpiry = "eco_boost_expiry";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // --- Currencies ---
    public void AddCoins(long amount)       { Coins += amount;      Save(); }
    public bool SpendCoins(long amount)     { if (Coins < amount) return false; Coins -= amount; Save(); return true; }
    public void AddFreeSpins(int amount)    { FreeSpins += amount;  Save(); }
    public bool UseFreeSpins(int amount)    { if (FreeSpins < amount) return false; FreeSpins -= amount; Save(); return true; }
    public void AddSuperSpins(int amount)   { SuperSpins += amount; Save(); }
    public bool UseSuperSpins(int amount)   { if (SuperSpins < amount) return false; SuperSpins -= amount; Save(); return true; }
    public void AddUltraSpins(int amount)   { UltraSpins += amount; Save(); }
    public bool UseUltraSpins(int amount)   { if (UltraSpins < amount) return false; UltraSpins -= amount; Save(); return true; }

    // --- Cosmetics ---
    public void SetBadge(string id)        { BadgeId = id;     Save(); }
    public void SetReelSkin(string id)     { ReelSkinId = id;  Save(); }
    public void SetProfileImage(string url) { ProfileImageUrl = url; Save(); }

    // --- Boost ---
    public void ApplyWinBoost(float multiplier, double durationSeconds)
    {
        WinMultiplier = multiplier;
        _boostExpiryTime = GetUtcNow() + durationSeconds;
        Save();
    }

    public float GetActiveMultiplier()
    {
        if (GetUtcNow() < _boostExpiryTime) return WinMultiplier;
        if (WinMultiplier != 1f) { WinMultiplier = 1f; Save(); }
        return 1f;
    }

    // --- Persistence ---
    public void Save()
    {
        PlayerPrefs.SetString(KeyCoins,      Coins.ToString());
        PlayerPrefs.SetInt(KeyFreeSpins,     FreeSpins);
        PlayerPrefs.SetInt(KeySuperSpins,    SuperSpins);
        PlayerPrefs.SetInt(KeyUltraSpins,    UltraSpins);
        PlayerPrefs.SetString(KeyBadge,      BadgeId ?? "");
        PlayerPrefs.SetString(KeyReelSkin,   ReelSkinId ?? "");
        PlayerPrefs.SetString(KeyProfileImg, ProfileImageUrl ?? "");
        PlayerPrefs.SetFloat(KeyMultiplier,  WinMultiplier);
        PlayerPrefs.SetString(KeyBoostExpiry, _boostExpiryTime.ToString());
        PlayerPrefs.Save();
    }

    private void Load()
    {
        bool firstRun = !PlayerPrefs.HasKey(KeyCoins);
        Coins        = long.TryParse(PlayerPrefs.GetString(KeyCoins, "0"), out long c) ? c : 0;
        if (firstRun) { Coins = StartingCoins; Save(); }
        FreeSpins    = PlayerPrefs.GetInt(KeyFreeSpins, 0);
        SuperSpins   = PlayerPrefs.GetInt(KeySuperSpins, 0);
        UltraSpins   = PlayerPrefs.GetInt(KeyUltraSpins, 0);
        BadgeId      = PlayerPrefs.GetString(KeyBadge, "");
        ReelSkinId   = PlayerPrefs.GetString(KeyReelSkin, "");
        ProfileImageUrl = PlayerPrefs.GetString(KeyProfileImg, "");
        WinMultiplier = PlayerPrefs.GetFloat(KeyMultiplier, 1f);
        _boostExpiryTime = double.TryParse(PlayerPrefs.GetString(KeyBoostExpiry, "0"), out double exp) ? exp : 0;
    }

    private static double GetUtcNow() =>
        (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
}
