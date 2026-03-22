using System;
using UnityEngine;

/// <summary>
/// Gem hard currency system.
/// Gems are earned through gameplay milestones and purchased via IAP.
/// Players can spend gems to buy Free Spins, Super Spins, and Ultra Spins
/// in the currently active slot game.
/// </summary>
public class GemSystem : MonoBehaviour
{
    public static GemSystem Instance { get; private set; }

    public int Gems { get; private set; }

    [Header("Spin Costs (in Gems)")]
    public int costFreeSpinPerGem   = 10;   // 10 gems → 1 free spin
    public int costSuperSpinPerGem  = 30;   // 30 gems → 1 super spin
    public int costUltraSpinPerGem  = 100;  // 100 gems → 1 ultra spin

    [Header("Gem Bundles (IAP)")]
    public int[] gemBundleAmounts = { 100, 500, 1200, 2500, 6500 };
    public string[] gemBundleProductIds = {
        "gems_100", "gems_500", "gems_1200", "gems_2500", "gems_6500"
    };

    private const string KeyGems = "gem_balance";

    public static event Action<int> OnGemsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Gems = PlayerPrefs.GetInt(KeyGems, 0);
    }

    // ── Economy ───────────────────────────────────────────────────────────────
    public void AddGems(int amount)
    {
        Gems += amount;
        Save();
        OnGemsChanged?.Invoke(Gems);
    }

    public bool SpendGems(int amount)
    {
        if (Gems < amount) return false;
        Gems -= amount;
        Save();
        OnGemsChanged?.Invoke(Gems);
        return true;
    }

    // ── Spin purchases ────────────────────────────────────────────────────────

    /// <summary>Buy <paramref name="count"/> free spins using gems.</summary>
    public bool BuyFreeSpins(int count = 1)
    {
        int cost = costFreeSpinPerGem * count;
        if (!SpendGems(cost)) return false;
        PlayerEconomy.Instance?.AddFreeSpins(count);
        Debug.Log($"[GemSystem] Bought {count} Free Spin(s) for {cost} gems.");
        return true;
    }

    /// <summary>Buy <paramref name="count"/> super spins using gems.</summary>
    public bool BuySuperSpins(int count = 1)
    {
        int cost = costSuperSpinPerGem * count;
        if (!SpendGems(cost)) return false;
        PlayerEconomy.Instance?.AddSuperSpins(count);
        Debug.Log($"[GemSystem] Bought {count} Super Spin(s) for {cost} gems.");
        return true;
    }

    /// <summary>Buy <paramref name="count"/> ultra spins using gems.</summary>
    public bool BuyUltraSpins(int count = 1)
    {
        int cost = costUltraSpinPerGem * count;
        if (!SpendGems(cost)) return false;
        PlayerEconomy.Instance?.AddUltraSpins(count);
        Debug.Log($"[GemSystem] Bought {count} Ultra Spin(s) for {cost} gems.");
        return true;
    }

    // ── IAP grant (called by IAPHandler on purchase complete) ─────────────────
    public void OnIAPBundlePurchased(string productId)
    {
        for (int i = 0; i < gemBundleProductIds.Length; i++)
        {
            if (gemBundleProductIds[i] == productId)
            {
                AddGems(gemBundleAmounts[i]);
                Debug.Log($"[GemSystem] IAP: +{gemBundleAmounts[i]} gems from {productId}");
                return;
            }
        }
    }

    private void Save()
    {
        PlayerPrefs.SetInt(KeyGems, Gems);
        PlayerPrefs.Save();
    }
}
