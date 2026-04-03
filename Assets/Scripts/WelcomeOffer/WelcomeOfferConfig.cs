using UnityEngine;

[CreateAssetMenu(fileName = "WelcomeOfferConfig", menuName = "SlotApp/WelcomeOfferConfig")]
public class WelcomeOfferConfig : ScriptableObject
{
    [Header("Currencies")]
    public long coins = 150_000_000_000L;
    public int freeSpins = 200;
    public int superSpins = 25;
    public int ultraSpins = 1;
    public int jackpotTickets = 10;

    [Header("Boost")]
    public float winBoostMultiplier = 2f;
    public float winBoostDurationHours = 24f;

    [Header("Cosmetics")]
    public string badgeId = "vip_badge";
    public string reelSkinId = "golden_reel";

    [Header("Mystery Chests")]
    public int mysteryChests = 3;

    [Header("Price Display")]
    public string priceLabel = "$4.99";
    public string productId = "welcomeofferpack";
}
