using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main lobby screen controller.
///
/// Layout (top → bottom):
///   ┌──────────────────────────────────────┐
///   │  CasinoHUD  (persistent, separate)   │
///   ├──────────────────────────────────────┤
///   │  [BANNER CAROUSEL] (LobbyBannerUI)   │
///   ├──────────────────────────────────────┤
///   │  SEARCH BAR  │  FILTER TABS          │
///   ├──────────────────────────────────────┤
///   │  FEATURED / HOT ROW  (horizontal)    │
///   ├──────────────────────────────────────┤
///   │  ALL GAMES  2-column scrollable grid │
///   └──────────────────────────────────────┘
///   │  BottomNavBar  (persistent, separate)│
///
/// Assign <see cref="gameCardPrefab"/> to a prefab that has a
/// <see cref="GameCardUI"/> component on its root.
/// </summary>
public class LobbyController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    [Header("Game Card")]
    [SerializeField] private GameCardUI gameCardPrefab;

    [Header("Featured Row")]
    [SerializeField] private Transform  featuredContent;   // HorizontalLayoutGroup parent
    [SerializeField] private ScrollRect featuredScroll;

    [Header("Main Grid")]
    [SerializeField] private Transform  gridContent;       // GridLayoutGroup parent
    [SerializeField] private ScrollRect mainScroll;

    [Header("Search & Filter")]
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private Button tabAll;
    [SerializeField] private Button tabFeatured;
    [SerializeField] private Button tabNew;
    [SerializeField] private Button tabJackpot;
    [SerializeField] private Button tabFreeSpins;

    [Header("Loading / Empty State")]
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private TMP_Text   emptyStateLabel;

    [Header("Game Count Label")]
    [SerializeField] private TMP_Text gameCountLabel;

    // ── state ─────────────────────────────────────────────────────────────

    private enum FilterTab { All, Featured, New, Jackpot, FreeSpins }
    private FilterTab _activeTab = FilterTab.All;
    private string    _searchQuery = "";

    // First N game IDs that are "featured" (shown in horizontal strip and tagged NEW/HOT)
    private static readonly HashSet<string> FeaturedIds = new HashSet<string>
    {
        "spirit_buffalo_run", "prairie_fire_spirit", "painted_mustang_spirit",
        "thunderhawk_sentinel", "golden_coyote_watch"
    };

    // Game IDs locked until the player reaches a certain level (demo: last 5)
    private static readonly HashSet<string> LockedIds = new HashSet<string>
    {
        "shadow_serpent_coils", "crystal_bear_medicine",
        "rainbow_crow_nation",  "glacier_mammoth_stomp", "sunfire_snake_dancer"
    };

    private readonly List<GameCardUI> _allCards     = new List<GameCardUI>();
    private readonly List<GameCardUI> _featuredCards = new List<GameCardUI>();

    // ── lifecycle ─────────────────────────────────────────────────────────

    private void Start()
    {
        WireTabButtons();
        if (searchField != null)
            searchField.onValueChanged.AddListener(OnSearchChanged);

        StartCoroutine(BuildLobby());
    }

    // ── coroutine: build all cards ────────────────────────────────────────

    private IEnumerator BuildLobby()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        yield return null;   // one frame to render the spinner

        // ---- featured row ----
        if (featuredContent != null && gameCardPrefab != null)
        {
            foreach (SlotGameDatabase.GameDefinition def in SlotGameDatabase.All)
            {
                if (!FeaturedIds.Contains(def.GameId)) continue;

                GameCardUI card = Instantiate(gameCardPrefab, featuredContent);
                card.Populate(def, false, theme);
                _featuredCards.Add(card);
            }
        }

        // ---- main grid ----
        if (gridContent != null && gameCardPrefab != null)
        {
            foreach (SlotGameDatabase.GameDefinition def in SlotGameDatabase.All)
            {
                bool locked = LockedIds.Contains(def.GameId);
                GameCardUI card = Instantiate(gameCardPrefab, gridContent);
                card.Populate(def, locked, theme);
                _allCards.Add(card);
                yield return null;  // spread instantiation over multiple frames
            }
        }

        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        ApplyFilter();
    }

    // ── public: called by GameCardUI ─────────────────────────────────────

    public void OnGameCardSelected(SlotGameDatabase.GameDefinition def)
    {
        // Pass the chosen game ID to the slot scene loader
        SlotGameLoader loader = FindObjectOfType<SlotGameLoader>();
        if (loader != null)
            loader.LoadGameById(def.GameId);
        else
            Debug.LogWarning($"[LobbyController] No SlotGameLoader found to launch '{def.GameId}'");
    }

    // ── filter / search ───────────────────────────────────────────────────

    private void WireTabButtons()
    {
        WireTab(tabAll,        FilterTab.All);
        WireTab(tabFeatured,   FilterTab.Featured);
        WireTab(tabNew,        FilterTab.New);
        WireTab(tabJackpot,    FilterTab.Jackpot);
        WireTab(tabFreeSpins,  FilterTab.FreeSpins);
    }

    private void WireTab(Button btn, FilterTab tab)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() =>
        {
            _activeTab = tab;
            ApplyFilter();
            HighlightTab(btn);
        });
    }

    private void HighlightTab(Button selected)
    {
        Button[] allTabs = { tabAll, tabFeatured, tabNew, tabJackpot, tabFreeSpins };
        foreach (Button b in allTabs)
        {
            if (b == null) continue;
            ColorBlock cb = b.colors;
            cb.normalColor = b == selected
                ? (theme != null ? theme.goldPrimary : Color.yellow)
                : new Color(0.15f, 0.12f, 0.30f);
            b.colors = cb;
        }
    }

    private void OnSearchChanged(string value)
    {
        _searchQuery = value.Trim().ToLowerInvariant();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        int visible = 0;
        foreach (GameCardUI card in _allCards)
        {
            bool show = MatchesFilter(card) && MatchesSearch(card);
            card.gameObject.SetActive(show);
            if (show) visible++;
        }

        if (gameCountLabel != null)
            gameCountLabel.text = $"{visible} Games";

        if (emptyStatePanel != null)
        {
            bool empty = visible == 0;
            emptyStatePanel.SetActive(empty);
            if (empty && emptyStateLabel != null)
                emptyStateLabel.text = string.IsNullOrEmpty(_searchQuery)
                    ? "No games in this category yet."
                    : $"No results for \"{_searchQuery}\"";
        }

        // Also filter featured row by current search
        foreach (GameCardUI card in _featuredCards)
            card.gameObject.SetActive(MatchesSearch(card));
    }

    private bool MatchesFilter(GameCardUI card)
    {
        // Retrieve the definition stored in card via a tiny helper
        SlotGameDatabase.GameDefinition def = card.GetDefinition();
        return _activeTab switch
        {
            FilterTab.Featured  => FeaturedIds.Contains(def.GameId),
            FilterTab.New       => FeaturedIds.Contains(def.GameId),   // first 5 are also "new"
            FilterTab.Jackpot   => def.HasProgressive,
            FilterTab.FreeSpins => def.HasFreeSpins,
            _                   => true   // All
        };
    }

    private bool MatchesSearch(GameCardUI card)
    {
        if (string.IsNullOrEmpty(_searchQuery)) return true;
        SlotGameDatabase.GameDefinition def = card.GetDefinition();
        return def.GameName.ToLowerInvariant().Contains(_searchQuery)
            || def.Theme.ToLowerInvariant().Contains(_searchQuery);
    }
}
