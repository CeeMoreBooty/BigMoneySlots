using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Persistent top-of-screen HUD: coin balance, gem balance, player level,
/// jackpot ticker, and daily bonus shortcut.
/// Attach to a full-screen Canvas that uses DontDestroyOnLoad.
/// </summary>
public class CasinoHUD : MonoBehaviour
{
    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    [Header("Coin Display")]
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text gemsLabel;
    [SerializeField] private Image    coinIcon;
    [SerializeField] private Image    gemIcon;

    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameLabel;
    [SerializeField] private TMP_Text playerLevelLabel;
    [SerializeField] private Slider   xpBar;

    [Header("Jackpot Ticker")]
    [SerializeField] private TMP_Text jackpotLabel;
    [SerializeField] private GameObject jackpotPanel;

    [Header("Buttons")]
    [SerializeField] private Button addCoinsButton;
    [SerializeField] private Button dailyBonusButton;
    [SerializeField] private Button settingsButton;

    [Header("Daily Bonus Badge")]
    [SerializeField] private GameObject dailyBonusBadge;   // "!" dot shown when bonus is available

    // ── cached last values to avoid text rebuild every frame ──────────────
    private long   _lastCoins   = long.MinValue;
    private long   _lastGems    = long.MinValue;
    private long   _lastJackpot = long.MinValue;
    private float  _jackpotAnim;

    // ── coin-burst animation bookkeeping ──────────────────────────────────
    private long   _displayedCoins;
    private long   _targetCoins;
    private float  _coinCountTimer;
    private const float CoinCountDuration = 0.8f;

    // ── lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Start()
    {
        if (addCoinsButton)  addCoinsButton.onClick.AddListener(OnAddCoins);
        if (dailyBonusButton) dailyBonusButton.onClick.AddListener(OnDailyBonus);
        if (settingsButton)  settingsButton.onClick.AddListener(OnSettings);

        RefreshAll();
    }

    private void Update()
    {
        TickCoinCounter();
        TickJackpot();
        RefreshDailyBonusBadge();
    }

    // ── public API ────────────────────────────────────────────────────────

    /// <summary>Animate the coin counter from its current value to <paramref name="newAmount"/>.</summary>
    public void AnimateCoinChange(long newAmount)
    {
        _targetCoins    = newAmount;
        _coinCountTimer = CoinCountDuration;
    }

    // ── private helpers ───────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (PlayerEconomy.Instance != null)
        {
            _displayedCoins = PlayerEconomy.Instance.Coins;
            _targetCoins    = _displayedCoins;
            UpdateCoinText(_displayedCoins);
        }

        if (GemSystem.Instance != null && gemsLabel != null)
            gemsLabel.text = GemSystem.Instance.Gems.ToString();

        if (playerNameLabel != null)
            playerNameLabel.text = PlayerPrefs.GetString("PlayerName", "Player");

        if (playerLevelLabel != null)
            playerLevelLabel.text = $"Lv.{PlayerPrefs.GetInt("PlayerLevel", 1)}";

        if (xpBar != null)
        {
            float xp    = PlayerPrefs.GetFloat("XP", 0f);
            float xpMax = PlayerPrefs.GetFloat("XPMax", 1000f);
            xpBar.value = Mathf.Clamp01(xp / xpMax);
        }
    }

    private void TickCoinCounter()
    {
        if (_coinCountTimer <= 0f) return;
        _coinCountTimer -= Time.deltaTime;

        float t = 1f - Mathf.Clamp01(_coinCountTimer / CoinCountDuration);
        // ease-out
        t = 1f - (1f - t) * (1f - t);
        _displayedCoins = (long)Mathf.Lerp(_lastCoins == long.MinValue ? 0 : (float)_lastCoins,
                                            (float)_targetCoins, t);

        UpdateCoinText(_displayedCoins);

        if (_coinCountTimer <= 0f)
        {
            _displayedCoins = _targetCoins;
            UpdateCoinText(_displayedCoins);
        }
    }

    private void TickJackpot()
    {
        if (ProgressiveJackpot.Instance == null) return;

        long jackpot = ProgressiveJackpot.Instance.CurrentJackpot;
        if (jackpot != _lastJackpot)
        {
            _lastJackpot = jackpot;
            if (jackpotLabel != null)
                jackpotLabel.text = $"🎰 JACKPOT  {FormatCoins(jackpot)}";
        }

        // gentle bob
        _jackpotAnim += Time.deltaTime * 1.8f;
        if (jackpotPanel != null)
        {
            RectTransform rt = jackpotPanel.GetComponent<RectTransform>();
            if (rt) rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,
                                                       Mathf.Sin(_jackpotAnim) * 3f);
        }
    }

    private void RefreshDailyBonusBadge()
    {
        if (dailyBonusBadge == null) return;
        DailyBonus db = FindObjectOfType<DailyBonus>();
        dailyBonusBadge.SetActive(db != null && db.CanClaimToday);
    }

    private void UpdateCoinText(long amount)
    {
        if (coinsLabel != null)
            coinsLabel.text = FormatCoins(amount);
    }

    private void OnAddCoins()
    {
        // Opens the IAP / coin-store panel — implemented by WelcomeOfferUI
        WelcomeOfferUI wou = FindObjectOfType<WelcomeOfferUI>();
        if (wou != null) wou.Show();
    }

    private void OnDailyBonus()
    {
        DailyBonus db = FindObjectOfType<DailyBonus>();
        if (db != null) { db.CheckAndAward(); db.Collect(); }
    }

    private void OnSettings()
    {
        // Toggle settings panel — hooked up in scene via the settings panel GameObject
        GameObject panel = GameObject.Find("SettingsPanel");
        if (panel != null) panel.SetActive(!panel.activeSelf);
    }

    private static string FormatCoins(long amount)
    {
        if (amount >= 1_000_000_000_000L) return $"{amount / 1_000_000_000_000.0:0.##}T";
        if (amount >= 1_000_000_000L)     return $"{amount / 1_000_000_000.0:0.##}B";
        if (amount >= 1_000_000L)         return $"{amount / 1_000_000.0:0.##}M";
        if (amount >= 1_000L)             return $"{amount / 1_000.0:0.##}K";
        return amount.ToString();
    }
}
