using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// One tile in the game-selection lobby grid.
/// Drives the card background colour, game name/theme labels,
/// feature badge strip, and hover/press animation.
/// </summary>
[RequireComponent(typeof(Button))]
public class GameCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                         IPointerDownHandler, IPointerUpHandler
{
    [Header("Labels")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text themeLabel;
    [SerializeField] private TMP_Text descLabel;

    [Header("Visuals")]
    [SerializeField] private Image     cardBackground;
    [SerializeField] private Image     accentStripe;       // top coloured stripe
    [SerializeField] private Image     iconImage;          // game thumbnail / placeholder
    [SerializeField] private GameObject lockedOverlay;     // darkened with lock icon
    [SerializeField] private TMP_Text  lockedLabel;

    [Header("Badges")]
    [SerializeField] private GameObject badgeNew;
    [SerializeField] private GameObject badgeHot;
    [SerializeField] private GameObject badgeJackpot;
    [SerializeField] private GameObject badgeFreeSpins;
    [SerializeField] private GameObject badgeBonus;

    [Header("Feature Icons")]
    [SerializeField] private GameObject wildIcon;
    [SerializeField] private GameObject scatterIcon;
    [SerializeField] private GameObject freeSpinsIcon;
    [SerializeField] private GameObject bonusIcon;
    [SerializeField] private GameObject progressiveIcon;

    [Header("Volatility Bar")]
    [SerializeField] private Image[] volatilityDots;    // 4 dots filled = VeryHigh

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    // ── runtime ───────────────────────────────────────────────────────────
    private SlotGameDatabase.GameDefinition _def;
    private bool _locked;
    private RectTransform _rt;
    private Vector3 _baseScale;

    // ── public initialiser ────────────────────────────────────────────────

    public void Populate(SlotGameDatabase.GameDefinition def, bool locked, CasinoTheme t)
    {
        _def    = def;
        _locked = locked;
        theme   = t;

        _rt        = GetComponent<RectTransform>();
        _baseScale = transform.localScale;

        // --- labels ---
        if (nameLabel  != null) nameLabel.text  = def.GameName;
        if (themeLabel != null) themeLabel.text = def.Theme;
        if (descLabel  != null) descLabel.text  = def.Description;

        // --- card background accent tint ---
        if (ColorUtility.TryParseHtmlString(def.BgColor, out Color bg))
        {
            if (cardBackground != null) cardBackground.color = bg;
        }
        if (ColorUtility.TryParseHtmlString(def.AccentColor, out Color acc))
        {
            if (accentStripe != null) accentStripe.color = acc;
            if (nameLabel    != null) nameLabel.color    = acc;
        }

        // --- feature badges ---
        SetActive(badgeFreeSpins, def.HasFreeSpins);
        SetActive(badgeBonus,     def.HasBonus);
        SetActive(badgeJackpot,   def.HasProgressive);

        // --- feature icons ---
        SetActive(wildIcon,        def.HasWild);
        SetActive(scatterIcon,     def.HasScatter);
        SetActive(freeSpinsIcon,   def.HasFreeSpins);
        SetActive(bonusIcon,       def.HasBonus);
        SetActive(progressiveIcon, def.HasProgressive);

        // --- volatility dots ---
        int vLevel = (int)def.Volatility;   // 0=Low … 3=VeryHigh
        if (volatilityDots != null)
        {
            for (int i = 0; i < volatilityDots.Length; i++)
            {
                if (volatilityDots[i] == null) continue;
                Color dotColor = i <= vLevel
                    ? VolatilityColor((SlotGameConfig.Volatility)vLevel)
                    : new Color(0.25f, 0.22f, 0.35f);
                volatilityDots[i].color = dotColor;
            }
        }

        // --- locked state ---
        if (lockedOverlay != null) lockedOverlay.SetActive(locked);
        if (lockedLabel   != null) lockedLabel.text = locked ? "🔒 Coming Soon" : "";

        // --- button wiring ---
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        if (!locked)
            btn.onClick.AddListener(OnCardClicked);
    }

    // ── hover / press animation ───────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
    {
        if (_locked) return;
        float scale = theme != null ? theme.cardHoverScale : 1.06f;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * scale, 0.12f));
    }

    public void OnPointerExit(PointerEventData _)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(_baseScale, 0.12f));
    }

    public void OnPointerDown(PointerEventData _)
    {
        if (_locked) return;
        float scale = theme != null ? theme.cardPressScale : 0.94f;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * scale, 0.06f));
    }

    public void OnPointerUp(PointerEventData _)
    {
        if (_locked) return;
        float scale = theme != null ? theme.cardHoverScale : 1.06f;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * scale, 0.08f));
    }

    // ── click handler ─────────────────────────────────────────────────────

    private void OnCardClicked()
    {
        LobbyController lobby = FindObjectOfType<LobbyController>();
        if (lobby != null) lobby.OnGameCardSelected(_def);
    }

    // ── public accessors ─────────────────────────────────────────────────

    /// <summary>Returns the game definition this card was populated with.</summary>
    public SlotGameDatabase.GameDefinition GetDefinition() => _def;

    // ── helpers ───────────────────────────────────────────────────────────

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null) go.SetActive(state);
    }

    private static Color VolatilityColor(SlotGameConfig.Volatility v) => v switch
    {
        SlotGameConfig.Volatility.Low      => new Color(0.20f, 0.85f, 0.20f),  // green
        SlotGameConfig.Volatility.Medium   => new Color(1.00f, 0.80f, 0.00f),  // gold
        SlotGameConfig.Volatility.High     => new Color(1.00f, 0.40f, 0.00f),  // orange
        SlotGameConfig.Volatility.VeryHigh => new Color(1.00f, 0.10f, 0.10f),  // red
        _                                  => Color.white
    };

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float   t     = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.localScale = Vector3.LerpUnclamped(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }
}
