using UnityEngine;
using TMPro;

/// <summary>
/// Central HUD controller — bridges multiple UI sub-panels and surfaces
/// toasts / banners for rewards, tier-ups, and milestone events.
/// Works alongside UIBuilder: UIBuilder creates the geometry; HUDManager
/// listens to gameplay events and writes into the text elements.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Optional toast display")]
    [Tooltip("Assign a TMP_Text in the Canvas for floating reward announcements.")]
    public TextMeshProUGUI toastText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LoyaltySystem.OnTierUp          += OnTierUp;
        LoyaltySystem.OnMilestoneReached += OnMilestone;
        LoyaltySystem.OnComebackBonus    += OnComeback;
        LoyaltySystem.OnSessionReward    += OnSessionReward;
        ProgressiveJackpot.OnJackpotWon  += OnJackpotWon;
    }

    private void OnDisable()
    {
        LoyaltySystem.OnTierUp          -= OnTierUp;
        LoyaltySystem.OnMilestoneReached -= OnMilestone;
        LoyaltySystem.OnComebackBonus    -= OnComeback;
        LoyaltySystem.OnSessionReward    -= OnSessionReward;
        ProgressiveJackpot.OnJackpotWon  -= OnJackpotWon;
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    private void OnTierUp(LoyaltySystem.LoyaltyTier tier)
        => ShowToast($"🏅 TIER UP!  You are now a {tier}!", 4f);

    private void OnMilestone(long coins, int free, int super)
        => ShowToast($"🎯 MILESTONE!  +{FormatCoins(coins)} coins  +{free} free spins!", 5f);

    private void OnComeback()
        => ShowToast("🎉 Welcome back!  Comeback bonus added!", 4f);

    private void OnSessionReward(long coins, int freeSpins)
        => ShowToast($"⏰ 30-min session reward!  +{FormatCoins(coins)} coins!", 3f);

    private void OnJackpotWon(ProgressiveJackpot.JackpotTier tier, long amount, string label)
        => ShowToast($"🎰🎰🎰  {label} JACKPOT!  +{FormatCoins(amount)}  🎰🎰🎰", 6f);

    // ── Toast ─────────────────────────────────────────────────────────────────
    public void ShowToast(string message, float duration = 3f)
    {
        Debug.Log($"[HUD] {message}");
        if (toastText != null)
        {
            toastText.text = message;
            CancelInvoke(nameof(ClearToast));
            Invoke(nameof(ClearToast), duration);
        }
    }

    private void ClearToast()
    {
        if (toastText != null) toastText.text = "";
    }

    // ── Format helpers ────────────────────────────────────────────────────────
    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000_000L) return $"{v / 1_000_000_000_000L}T";
        if (v >= 1_000_000_000L)     return $"{v / 1_000_000_000L}B";
        if (v >= 1_000_000L)         return $"{v / 1_000_000L}M";
        if (v >= 1_000L)             return $"{v / 1_000L}K";
        return v.ToString();
    }
}
