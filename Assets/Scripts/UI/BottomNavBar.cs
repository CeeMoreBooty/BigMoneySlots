using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bottom navigation bar with five tabs:
///   Home | Games | Jackpots | Rewards | Profile
///
/// Attach to a Canvas/Panel anchored to the bottom of the screen.
/// Wire each tab button in the Inspector.
/// </summary>
public class BottomNavBar : MonoBehaviour
{
    public enum Tab { Home, Games, Jackpots, Rewards, Profile }

    [System.Serializable]
    public struct TabEntry
    {
        public Button    button;
        public Image     iconImage;
        public TMP_Text  label;
        public GameObject badge;  // notification dot
    }

    [Header("Tabs")]
    [SerializeField] private TabEntry tabHome;
    [SerializeField] private TabEntry tabGames;
    [SerializeField] private TabEntry tabJackpots;
    [SerializeField] private TabEntry tabRewards;
    [SerializeField] private TabEntry tabProfile;

    [Header("Panels to show/hide")]
    [SerializeField] private GameObject panelHome;
    [SerializeField] private GameObject panelGames;
    [SerializeField] private GameObject panelJackpots;
    [SerializeField] private GameObject panelRewards;
    [SerializeField] private GameObject panelProfile;

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    private Tab _activeTab = Tab.Home;

    // ── lifecycle ─────────────────────────────────────────────────────────

    private void Start()
    {
        Wire(tabHome,     Tab.Home);
        Wire(tabGames,    Tab.Games);
        Wire(tabJackpots, Tab.Jackpots);
        Wire(tabRewards,  Tab.Rewards);
        Wire(tabProfile,  Tab.Profile);

        SelectTab(Tab.Home);
    }

    // ── public API ────────────────────────────────────────────────────────

    public void SelectTab(Tab tab)
    {
        _activeTab = tab;

        SetPanel(panelHome,     tab == Tab.Home);
        SetPanel(panelGames,    tab == Tab.Games);
        SetPanel(panelJackpots, tab == Tab.Jackpots);
        SetPanel(panelRewards,  tab == Tab.Rewards);
        SetPanel(panelProfile,  tab == Tab.Profile);

        HighlightTab(tabHome,     tab == Tab.Home);
        HighlightTab(tabGames,    tab == Tab.Games);
        HighlightTab(tabJackpots, tab == Tab.Jackpots);
        HighlightTab(tabRewards,  tab == Tab.Rewards);
        HighlightTab(tabProfile,  tab == Tab.Profile);
    }

    public void ShowRewardsBadge(bool show) => SetBadge(tabRewards, show);
    public void ShowProfileBadge(bool show) => SetBadge(tabProfile, show);

    // ── private helpers ───────────────────────────────────────────────────

    private void Wire(TabEntry entry, Tab tab)
    {
        if (entry.button == null) return;
        entry.button.onClick.AddListener(() => SelectTab(tab));
    }

    private void HighlightTab(TabEntry entry, bool active)
    {
        Color on  = theme != null ? theme.goldPrimary : Color.yellow;
        Color off = theme != null ? theme.textMuted   : new Color(0.55f, 0.52f, 0.65f);

        if (entry.iconImage != null) entry.iconImage.color = active ? on : off;
        if (entry.label     != null) entry.label.color     = active ? on : off;
    }

    private static void SetPanel(GameObject panel, bool state)
    {
        if (panel != null) panel.SetActive(state);
    }

    private static void SetBadge(TabEntry entry, bool state)
    {
        if (entry.badge != null) entry.badge.SetActive(state);
    }
}
