using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-scrolling promotional banner carousel shown at the top of the lobby.
/// Cycles through a fixed list of banners (colour + headline + sub-text).
/// Tap a banner to fire its associated action (e.g. open welcome offer).
/// </summary>
public class LobbyBannerUI : MonoBehaviour
{
    [System.Serializable]
    public struct BannerData
    {
        public string headline;
        public string subText;
        public Color  bgColorA;
        public Color  bgColorB;
        public string ctaLabel;
    }

    [Header("Panels")]
    [SerializeField] private RectTransform bannerViewport;   // masked area
    [SerializeField] private Transform     bannerContainer;  // parent of all banner items

    [Header("Prefab")]
    [SerializeField] private RectTransform bannerItemPrefab;

    [Header("Dot Indicators")]
    [SerializeField] private Transform dotContainer;
    [SerializeField] private Image     dotPrefab;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float slideDuration   = 0.35f;

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    // ── built-in banners (no need for Inspector data, but can be overridden) ──
    private static readonly BannerData[] DefaultBanners =
    {
        new BannerData {
            headline  = "🎉 WELCOME BONUS",
            subText   = "Claim 10,000,000 FREE COINS now!",
            bgColorA  = new Color(0.55f, 0.10f, 0.00f),
            bgColorB  = new Color(0.10f, 0.04f, 0.22f),
            ctaLabel  = "CLAIM"
        },
        new BannerData {
            headline  = "🔥 HOT JACKPOT",
            subText   = "Progressive jackpot is heating up!",
            bgColorA  = new Color(0.70f, 0.35f, 0.00f),
            bgColorB  = new Color(0.20f, 0.06f, 0.00f),
            ctaLabel  = "PLAY NOW"
        },
        new BannerData {
            headline  = "⭐ DAILY BONUS",
            subText   = "Log in every day for bigger rewards!",
            bgColorA  = new Color(0.00f, 0.30f, 0.60f),
            bgColorB  = new Color(0.00f, 0.08f, 0.22f),
            ctaLabel  = "COLLECT"
        },
        new BannerData {
            headline  = "💎 VIP LOUNGE",
            subText   = "Unlock exclusive high-roller games!",
            bgColorA  = new Color(0.40f, 0.00f, 0.70f),
            bgColorB  = new Color(0.10f, 0.00f, 0.22f),
            ctaLabel  = "EXPLORE"
        },
        new BannerData {
            headline  = "🎰 NEW GAMES",
            subText   = "5 brand-new slot adventures added!",
            bgColorA  = new Color(0.00f, 0.50f, 0.20f),
            bgColorB  = new Color(0.00f, 0.12f, 0.08f),
            ctaLabel  = "DISCOVER"
        },
    };

    // ── runtime ───────────────────────────────────────────────────────────
    private int           _current;
    private float         _timer;
    private bool          _sliding;
    private Image[]       _dots;
    private RectTransform[] _items;
    private float         _bannerWidth;

    // ── lifecycle ─────────────────────────────────────────────────────────
    private void Start()
    {
        BuildBanners();
        BuildDots();
        ShowBanner(0, animate: false);
        _timer = displayDuration;
    }

    private void Update()
    {
        if (_sliding || DefaultBanners.Length <= 1) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            int next = (_current + 1) % DefaultBanners.Length;
            StartCoroutine(SlideToNext(next));
        }
    }

    // ── build ─────────────────────────────────────────────────────────────

    private void BuildBanners()
    {
        if (bannerContainer == null || bannerItemPrefab == null) return;

        _bannerWidth = bannerViewport != null ? bannerViewport.rect.width : 800f;
        _items = new RectTransform[DefaultBanners.Length];

        for (int i = 0; i < DefaultBanners.Length; i++)
        {
            BannerData data = DefaultBanners[i];
            RectTransform item = Instantiate(bannerItemPrefab, bannerContainer);
            item.sizeDelta = new Vector2(_bannerWidth, item.sizeDelta.y);

            // Background gradient approximation via Image color
            Image bg = item.GetComponent<Image>();
            if (bg != null) bg.color = Color.Lerp(data.bgColorA, data.bgColorB, 0.5f);

            // Headline
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0) texts[0].text = data.headline;
            if (texts.Length > 1) texts[1].text = data.subText;

            // CTA Button
            Button btn = item.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                TMP_Text btnLabel = btn.GetComponentInChildren<TMP_Text>(true);
                if (btnLabel != null) btnLabel.text = data.ctaLabel;
                int captured = i;
                btn.onClick.AddListener(() => OnBannerCTA(captured));
            }

            // Position off-screen to the right (except banner 0)
            item.anchoredPosition = new Vector2(i * _bannerWidth, 0f);
            _items[i] = item;
        }
    }

    private void BuildDots()
    {
        if (dotContainer == null || dotPrefab == null) return;
        _dots = new Image[DefaultBanners.Length];
        for (int i = 0; i < DefaultBanners.Length; i++)
        {
            Image dot = Instantiate(dotPrefab, dotContainer);
            _dots[i] = dot;
        }
        UpdateDots();
    }

    // ── display ───────────────────────────────────────────────────────────

    private void ShowBanner(int index, bool animate)
    {
        _current = index;
        if (!animate)
        {
            if (_items == null) return;
            for (int i = 0; i < _items.Length; i++)
                if (_items[i] != null)
                    _items[i].anchoredPosition = new Vector2((i - _current) * _bannerWidth, 0f);
        }
        UpdateDots();
    }

    private IEnumerator SlideToNext(int next)
    {
        _sliding = true;
        _timer   = displayDuration;

        float elapsed = 0f;
        float[] startX = new float[_items.Length];
        float[] endX   = new float[_items.Length];

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null) continue;
            startX[i] = _items[i].anchoredPosition.x;
            endX[i]   = (i - next) * _bannerWidth;
        }

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null) continue;
                _items[i].anchoredPosition = new Vector2(
                    Mathf.Lerp(startX[i], endX[i], t), 0f);
            }
            yield return null;
        }

        _current = next;
        UpdateDots();
        _sliding = false;
    }

    private void UpdateDots()
    {
        if (_dots == null) return;
        Color active   = theme != null ? theme.goldPrimary : Color.yellow;
        Color inactive = new Color(0.35f, 0.32f, 0.45f);
        for (int i = 0; i < _dots.Length; i++)
            if (_dots[i] != null)
                _dots[i].color = i == _current ? active : inactive;
    }

    // ── CTA handler ───────────────────────────────────────────────────────

    private void OnBannerCTA(int bannerIndex)
    {
        switch (bannerIndex)
        {
            case 0:
                WelcomeOfferUI wou = FindObjectOfType<WelcomeOfferUI>();
                if (wou != null) wou.Show();
                break;
            case 1:
                // scroll to jackpot games — handled by lobby filter
                LobbyController lobby = FindObjectOfType<LobbyController>();
                // trigger jackpot tab via message
                break;
            case 2:
                DailyBonus db = DailyBonus.Instance;
                if (db != null) db.CheckAndAward();
                break;
        }
    }
}
