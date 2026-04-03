using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main-menu / lobby controller.
///
/// Buttons:
///   PLAY          → load Game scene
///   LEADERBOARD   → future scene / panel
///   SHOP          → future scene / panel (IAP)
///   SETTINGS      → open SettingsUI panel
///   INVITE        → open InviteUI panel
///
/// Displays the player's current balance so they see their coins on the lobby.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button leaderboardButton;
    public Button shopButton;
    public Button settingsButton;
    public Button inviteButton;

    [Header("Balance Display")]
    public TMP_Text balanceText;
    public TMP_Text playerNameText;

    [Header("Panels (optional — opened from Lobby)")]
    public SettingsUI settingsPanel;

    private void Start()
    {
        playButton?.onClick.AddListener(OnPlay);
        leaderboardButton?.onClick.AddListener(OnLeaderboard);
        shopButton?.onClick.AddListener(OnShop);
        settingsButton?.onClick.AddListener(OnSettings);
        inviteButton?.onClick.AddListener(OnInvite);
    }

    private void Update()
    {
        if (balanceText != null && PlayerEconomy.Instance != null)
            balanceText.text = $"💰 {FormatCoins(PlayerEconomy.Instance.Coins)}";

        if (playerNameText != null)
            playerNameText.text = PlayerPrefs.GetString("player_name", "Player");
    }

    // ── Button handlers ───────────────────────────────────────────────────────
    private void OnPlay()
    {
        AnalyticsManager.Instance?.Track(AnalyticsManager.Event.SceneLoad,
            "{"scene":"Game"}");
        SceneLoader.Instance?.LoadGame();
    }

    private void OnLeaderboard()
    {
        Debug.Log("[LobbyUI] Leaderboard — coming soon.");
        // TODO: SceneLoader.Instance?.Load("Leaderboard");
    }

    private void OnShop()
    {
        Debug.Log("[LobbyUI] Shop — coming soon.");
        // TODO: SceneLoader.Instance?.Load("Shop");
    }

    private void OnSettings()
    {
        if (settingsPanel != null) settingsPanel.Toggle();
        else Debug.Log("[LobbyUI] Settings panel not assigned.");
    }

    private void OnInvite()
    {
        InviteRewardManager.Instance?.FetchMyCode();
        Debug.Log("[LobbyUI] Invite panel — wire InviteUI here.");
    }

    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000_000L) return $"{v / 1_000_000_000_000L}T";
        if (v >= 1_000_000_000L)     return $"{v / 1_000_000_000L}B";
        if (v >= 1_000_000L)         return $"{v / 1_000_000L}M";
        if (v >= 1_000L)             return $"{v / 1_000L}K";
        return v.ToString();
    }
}
