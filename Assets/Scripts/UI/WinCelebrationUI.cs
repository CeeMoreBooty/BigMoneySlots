using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Full-screen overlay shown on big wins, mega wins, epic wins, and jackpots.
///
/// Hierarchy expected (all optional — script degrades gracefully):
///   WinCelebrationUI (this)
///   ├── Background (Image – semi-transparent dark overlay + glow)
///   ├── WinTypeLabel  (TMP_Text – "BIG WIN" / "MEGA WIN" / "EPIC WIN" / "JACKPOT")
///   ├── AmountLabel   (TMP_Text – "+1,500,000")
///   ├── SubLabel      (TMP_Text – e.g. "x50 MULTIPLIER!")
///   ├── CoinBurst     (ParticleSystem – plays on show)
///   ├── Confetti      (ParticleSystem – plays on jackpot only)
///   └── CloseButton   (Button)
/// </summary>
public class WinCelebrationUI : MonoBehaviour
{
    [Header("Panels / Background")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image       glowBackground;

    [Header("Labels")]
    [SerializeField] private TMP_Text winTypeLabel;
    [SerializeField] private TMP_Text amountLabel;
    [SerializeField] private TMP_Text subLabel;
    [SerializeField] private TMP_Text multiplierLabel;

    [Header("Particles")]
    [SerializeField] private ParticleSystem coinBurst;
    [SerializeField] private ParticleSystem confetti;
    [SerializeField] private ParticleSystem goldenRays;

    [Header("Close")]
    [SerializeField] private Button closeButton;
    [SerializeField] private float  autoCloseDuration = 4f;

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    // ── bobbing state for win label ───────────────────────────────────────
    private float _bobTimer;
    private bool  _visible;
    private RectTransform _winTypeRT;

    // ── lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (winTypeLabel != null) _winTypeRT = winTypeLabel.GetComponent<RectTransform>();
        if (closeButton  != null) closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_visible) return;
        _bobTimer += Time.unscaledDeltaTime;
        if (_winTypeRT != null && theme != null)
        {
            float y = Mathf.Sin(_bobTimer * theme.winTextBobFrequency)
                      * theme.winTextBobAmplitude;
            _winTypeRT.anchoredPosition = new Vector2(_winTypeRT.anchoredPosition.x, y);
        }
    }

    // ── public API ────────────────────────────────────────────────────────

    public void ShowBigWin(long amount)       => Show("💥 BIG WIN!",   amount, "",
        theme != null ? theme.bigWinGlow  : new Color(1f, 0.7f, 0f, 0.85f));

    public void ShowMegaWin(long amount)      => Show("🔥 MEGA WIN!",  amount, "x25 Multiplier!",
        theme != null ? theme.megaWinGlow : new Color(1f, 0.2f, 0.8f, 0.9f));

    public void ShowEpicWin(long amount)      => Show("🎉 EPIC WIN!",  amount, "x100 Multiplier!",
        theme != null ? theme.epicWinGlow : new Color(0.4f, 0f, 1f, 0.95f));

    public void ShowJackpot(long amount)      => Show("🏆 JACKPOT!!!",  amount, "GRAND PROGRESSIVE!",
        theme != null ? theme.jackpotGlow : Color.yellow, jackpot: true);

    public void Hide()
    {
        StopAllCoroutines();
        _visible = false;
        gameObject.SetActive(false);
    }

    // ── private ───────────────────────────────────────────────────────────

    private void Show(string typeText, long amount, string sub, Color glow, bool jackpot = false)
    {
        gameObject.SetActive(true);
        _visible   = true;
        _bobTimer  = 0f;

        if (winTypeLabel  != null) winTypeLabel.text  = typeText;
        if (amountLabel   != null) amountLabel.text   = $"+{FormatCoins(amount)}";
        if (subLabel      != null) subLabel.text      = sub;
        if (glowBackground!= null) glowBackground.color = glow;

        if (coinBurst  != null) { coinBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); coinBurst.Play(); }
        if (confetti   != null) { confetti.gameObject.SetActive(jackpot); if (jackpot) confetti.Play(); }
        if (goldenRays != null) goldenRays.Play();

        StopAllCoroutines();
        StartCoroutine(FadeIn());
        StartCoroutine(AutoClose());
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.25f;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSecondsRealtime(autoCloseDuration);
        Hide();
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
