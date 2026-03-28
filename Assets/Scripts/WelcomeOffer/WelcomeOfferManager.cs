using UnityEngine;

public class WelcomeOfferManager : MonoBehaviour
{
    public static WelcomeOfferManager Instance { get; private set; }

    public WelcomeOfferConfig config;

    public bool IsPurchased => PlayerPrefs.GetInt(KeyPurchased, 0) == 1;

    private const string KeyPurchased = "welcome_offer_purchased";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GrantWelcomeOffer()
    {
        if (IsPurchased)
        {
            Debug.LogWarning("Welcome offer already purchased.");
            return;
        }

        if (config == null)
        {
            Debug.LogError("WelcomeOfferConfig not assigned.");
            return;
        }

        var eco = PlayerEconomy.Instance;
        if (eco == null)
        {
            Debug.LogError("PlayerEconomy not found.");
            return;
        }

        eco.AddCoins(config.coins);
        eco.AddFreeSpins(config.freeSpins);
        eco.AddSuperSpins(config.superSpins);
        eco.AddUltraSpins(config.ultraSpins);
        eco.ApplyWinBoost(config.winBoostMultiplier, config.winBoostDurationHours * 3600f);
        eco.SetBadge(config.badgeId);
        eco.SetReelSkin(config.reelSkinId);

        // Jackpot tickets and mystery chests — stored separately
        PlayerPrefs.SetInt("player_jackpot_tickets",
            PlayerPrefs.GetInt("player_jackpot_tickets", 0) + config.jackpotTickets);
        PlayerPrefs.SetInt("player_mystery_chests",
            PlayerPrefs.GetInt("player_mystery_chests", 0) + config.mysteryChests);

        PlayerPrefs.SetInt(KeyPurchased, 1);
        PlayerPrefs.Save();

        Debug.Log("Welcome offer granted!");
    }
}
