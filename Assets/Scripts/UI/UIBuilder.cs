using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Builds the complete in-game UI at runtime so no manual Inspector wiring is
/// needed.  Finds existing SlotUI and JackpotUI MonoBehaviours and fills in
/// every public field (TMP_Text, Button) they expose.
///
/// Layout (1920×1080 reference):
///   ┌─ TOP BAR (balance | bet | jackpot ticker) ─────────────────────────────┐
///   │  REEL PANEL  [sym] [sym] [sym]                                          │
///   │  RESULT TEXT (big, centered)                                            │
///   │  JACKPOT TIERS  MINI / MINOR / MAJOR / GRAND / MEGA                    │
///   └─ CONTROL BAR  [–] BET [$] [+]   [AUTO SPIN]   [  S P I N  ]  ─────────┘
/// </summary>
public class UIBuilder : MonoBehaviour
{
    // ── Colours ───────────────────────────────────────────────────────────────
    private static readonly Color COL_DARK    = new Color(0.05f, 0.02f, 0.08f, 0.95f);
    private static readonly Color COL_PANEL   = new Color(0.10f, 0.04f, 0.16f, 0.90f);
    private static readonly Color COL_GOLD    = new Color(1.00f, 0.84f, 0.00f, 1.00f);
    private static readonly Color COL_GREEN   = new Color(0.10f, 0.75f, 0.10f, 1.00f);
    private static readonly Color COL_RED     = new Color(0.85f, 0.10f, 0.10f, 1.00f);
    private static readonly Color COL_BTN_OFF = new Color(0.25f, 0.10f, 0.40f, 1.00f);

    // References discovered / built at runtime
    private SlotUI    _slotUI;
    private JackpotUI _jackpotUI;
    private SlotMachine _slotMachine;

    // Text elements we own (updated in Update)
    private TextMeshProUGUI[] _reelTexts;   // 3 reel symbol displays

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        _slotUI      = FindObjectOfType<SlotUI>();
        _jackpotUI   = FindObjectOfType<JackpotUI>();
        _slotMachine = FindObjectOfType<SlotMachine>();

        EnsureEventSystem();
        Canvas canvas = BuildCanvas();

        BuildBackground(canvas);
        BuildTopBar(canvas);
        BuildReelPanel(canvas);
        BuildResultPanel(canvas);
        BuildJackpotTierPanel(canvas);
        BuildControlBar(canvas);
    }

    private void Update()
    {
        // Update reel symbol display each frame.
        if (_reelTexts == null || _slotMachine == null) return;
        var reels = _slotMachine.reels;
        if (reels == null) return;
        for (int i = 0; i < _reelTexts.Length && i < reels.Length; i++)
        {
            if (_reelTexts[i] == null) continue;
            var sym = reels[i]?.CurrentSymbol;
            _reelTexts[i].text = sym != null ? sym.symbolName : "?";
        }

        // Space key as quick-spin shortcut in the Editor.
        if (Input.GetKeyDown(KeyCode.Space))
            _slotMachine?.Spin();
    }

    // ── Event System ──────────────────────────────────────────────────────────
    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    // ── Canvas ────────────────────────────────────────────────────────────────
    private static Canvas BuildCanvas()
    {
        var go     = new GameObject("GameCanvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // ── Background ────────────────────────────────────────────────────────────
    private static void BuildBackground(Canvas canvas)
    {
        var bg   = MakePanel(canvas.transform, "Background",
                             Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var img  = bg.AddComponent<Image>();
        img.color = COL_DARK;
    }

    // ── Top bar (balance / bet / jackpot) ─────────────────────────────────────
    private void BuildTopBar(Canvas canvas)
    {
        var bar = MakePanel(canvas.transform, "TopBar",
                            new Vector2(0, 1), new Vector2(1, 1),
                            new Vector2(0, -70), Vector2.zero);
        bar.AddComponent<Image>().color = COL_PANEL;

        var balTxt = MakeTMP(bar.transform, "BalanceText", "💰 Balance: —",
                             28, TextAlignmentOptions.Left,
                             new Vector2(0, 0), new Vector2(0.4f, 1));
        balTxt.color = COL_GOLD;

        var betTxt = MakeTMP(bar.transform, "BetText", "Bet: 100",
                             24, TextAlignmentOptions.Center,
                             new Vector2(0.35f, 0), new Vector2(0.65f, 1));
        betTxt.color = Color.white;

        var jpTxt = MakeTMP(bar.transform, "JackpotText", "🎰 JACKPOT: —",
                            26, TextAlignmentOptions.Right,
                            new Vector2(0.6f, 0), new Vector2(1, 1));
        jpTxt.color = COL_GOLD;

        // Wire into SlotUI
        if (_slotUI != null)
        {
            _slotUI.balanceText = balTxt;
            _slotUI.betText     = betTxt;
            _slotUI.jackpotText = jpTxt;
        }
    }

    // ── Reel panel (3 symbol boxes) ───────────────────────────────────────────
    private void BuildReelPanel(Canvas canvas)
    {
        var panel = MakePanel(canvas.transform, "ReelPanel",
                              new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.75f),
                              Vector2.zero, Vector2.zero);
        panel.AddComponent<Image>().color = COL_PANEL;

        _reelTexts = new TextMeshProUGUI[3];
        string[] labels = { "REEL 1", "REEL 2", "REEL 3" };
        for (int i = 0; i < 3; i++)
        {
            float x0 = i / 3f, x1 = (i + 1) / 3f;
            var reelBox = MakePanel(panel.transform, $"Reel{i}Box",
                                   new Vector2(x0 + 0.01f, 0.05f),
                                   new Vector2(x1 - 0.01f, 0.95f),
                                   Vector2.zero, Vector2.zero);
            var boxImg = reelBox.AddComponent<Image>();
            boxImg.color = COL_BTN_OFF;

            _reelTexts[i] = MakeTMP(reelBox.transform, $"ReelTxt{i}", labels[i],
                                    36, TextAlignmentOptions.Center,
                                    Vector2.zero, Vector2.one);
            _reelTexts[i].color     = COL_GOLD;
            _reelTexts[i].fontStyle = FontStyles.Bold;
        }
    }

    // ── Result text ───────────────────────────────────────────────────────────
    private void BuildResultPanel(Canvas canvas)
    {
        var rt = MakeTMP(canvas.transform, "ResultText", "",
                         40, TextAlignmentOptions.Center,
                         new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.46f));
        rt.color     = COL_GOLD;
        rt.fontStyle = FontStyles.Bold;

        if (_slotUI != null)
            _slotUI.resultText = rt;
    }

    // ── Jackpot tier panel ────────────────────────────────────────────────────
    private void BuildJackpotTierPanel(Canvas canvas)
    {
        var panel = MakePanel(canvas.transform, "JackpotTiers",
                              new Vector2(0, 0.25f), new Vector2(1, 0.36f),
                              Vector2.zero, Vector2.zero);
        panel.AddComponent<Image>().color = new Color(0.08f, 0.03f, 0.12f, 0.85f);

        string[] tierNames = { "MINI", "MINOR", "MAJOR", "GRAND", "MEGA" };
        ProgressiveJackpot.JackpotTier[] tiers = {
            ProgressiveJackpot.JackpotTier.Mini,
            ProgressiveJackpot.JackpotTier.Minor,
            ProgressiveJackpot.JackpotTier.Major,
            ProgressiveJackpot.JackpotTier.Grand,
            ProgressiveJackpot.JackpotTier.Mega,
        };

        var displays = new List<JackpotUI.TierDisplay>();
        for (int i = 0; i < 5; i++)
        {
            float x0 = i / 5f, x1 = (i + 1) / 5f;
            var cell = MakePanel(panel.transform, $"Tier{i}",
                                 new Vector2(x0 + 0.002f, 0.05f),
                                 new Vector2(x1 - 0.002f, 0.95f),
                                 Vector2.zero, Vector2.zero);
            cell.AddComponent<Image>().color = COL_PANEL;

            var poolTxt = MakeTMP(cell.transform, "Pool", $"{tierNames[i]}
—",
                                  20, TextAlignmentOptions.Center,
                                  Vector2.zero, Vector2.one);
            poolTxt.color = i == 4 ? COL_GOLD : Color.white;

            displays.Add(new JackpotUI.TierDisplay
            {
                tier     = tiers[i],
                poolText = poolTxt,
            });
        }

        if (_jackpotUI != null)
            _jackpotUI.tierDisplays = displays.ToArray();
    }

    // ── Control bar (bet controls + spin buttons) ─────────────────────────────
    private void BuildControlBar(Canvas canvas)
    {
        var bar = MakePanel(canvas.transform, "ControlBar",
                            Vector2.zero, new Vector2(1, 0.22f),
                            new Vector2(0, 10), new Vector2(0, 0));
        bar.AddComponent<Image>().color = COL_PANEL;

        // ── Bet decrease button ────────────────────────────────────────────────
        var betDecBtn = MakeButton(bar.transform, "BetDecBtn", "−",
                                   new Vector2(0.02f, 0.1f), new Vector2(0.12f, 0.9f),
                                   COL_BTN_OFF);
        betDecBtn.onClick.AddListener(() => {
            if (_slotMachine != null)
                _slotMachine.betAmount = System.Math.Max(100, _slotMachine.betAmount / 2);
        });

        // ── Bet value display ──────────────────────────────────────────────────
        var betDisplay = MakeTMP(bar.transform, "BetDisplay", "BET
100",
                                 22, TextAlignmentOptions.Center,
                                 new Vector2(0.12f, 0.05f), new Vector2(0.32f, 0.95f));
        betDisplay.color = Color.white;

        // ── Bet increase button ────────────────────────────────────────────────
        var betIncBtn = MakeButton(bar.transform, "BetIncBtn", "+",
                                   new Vector2(0.32f, 0.1f), new Vector2(0.42f, 0.9f),
                                   COL_BTN_OFF);
        betIncBtn.onClick.AddListener(() => {
            if (_slotMachine != null)
                _slotMachine.betAmount = System.Math.Min(10_000_000, _slotMachine.betAmount * 2);
        });

        // ── Auto Spin button ───────────────────────────────────────────────────
        var autoSpinBtnGO = MakeButton(bar.transform, "AutoSpinBtn", "AUTO SPIN",
                                       new Vector2(0.44f, 0.1f), new Vector2(0.64f, 0.9f),
                                       new Color(0.6f, 0.3f, 0f, 1f));
        autoSpinBtnGO.onClick.AddListener(() => _slotMachine?.ToggleAutoSpin());

        // ── SPIN button ────────────────────────────────────────────────────────
        var spinBtnGO = MakeButton(bar.transform, "SpinBtn", "🎰  S P I N",
                                   new Vector2(0.67f, 0.05f), new Vector2(0.98f, 0.95f),
                                   COL_GREEN);

        // Direct click → FraudPrevention check → Spin
        spinBtnGO.onClick.AddListener(() => {
            bool allowed = FraudPrevention.Instance == null ||
                           FraudPrevention.Instance.AllowSpin();
            if (!allowed) return;
            _slotMachine?.Spin();
        });

        // Wire SlotUI button refs so SlotUI.HandleAutoSpinChanged() can toggle text.
        var autoLabel = autoSpinBtnGO.GetComponentInChildren<TextMeshProUGUI>();
        if (_slotUI != null)
        {
            _slotUI.spinButton         = spinBtnGO.GetComponent<Button>();
            _slotUI.autoSpinButton     = autoSpinBtnGO.GetComponent<Button>();
            _slotUI.autoSpinButtonText = autoLabel;
        }

        // Keep bet display in sync every frame via a small helper on the bar GO.
        var betSync = bar.AddComponent<BetDisplaySync>();
        betSync.betText       = betDisplay;
        betSync.slotMachine   = _slotMachine;

        // Wire AnalyticsManager to spin events.
        if (_slotMachine != null)
            _slotMachine.OnSpinComplete += (payout, isJackpot) => {
                AnalyticsManager.Instance?.TrackSpin(
                    _slotMachine.betAmount, payout, isJackpot);
                FraudPrevention.Instance?.ValidateWin(_slotMachine.betAmount, payout);
            };
    }

    // ── UI Factory helpers ────────────────────────────────────────────────────
    private static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt          = go.AddComponent<RectTransform>();
        rt.anchorMin    = anchorMin;
        rt.anchorMax    = anchorMax;
        rt.offsetMin    = offsetMin;
        rt.offsetMax    = offsetMax;
        return go;
    }

    private static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
        float fontSize, TextAlignmentOptions align,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp          = go.AddComponent<TextMeshProUGUI>();
        tmp.text         = text;
        tmp.fontSize     = fontSize;
        tmp.alignment    = align;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(4, 4);
        rt.offsetMax = new Vector2(-4, -4);

        var img   = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f), 1f);
        cb.pressedColor = new Color(
            bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        btn.colors = cb;

        // Label child
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt       = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var tmp           = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 28;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = Color.white;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.enableWordWrapping = false;

        return btn;
    }
}

// ─── Tiny helper attached to the control bar ──────────────────────────────────
/// <summary>Keeps the "BET nnn" label in sync with SlotMachine.betAmount.</summary>
internal class BetDisplaySync : MonoBehaviour
{
    public TextMeshProUGUI betText;
    public SlotMachine     slotMachine;

    private long _lastBet = -1;

    private void Update()
    {
        if (slotMachine == null || betText == null) return;
        if (slotMachine.betAmount == _lastBet) return;
        _lastBet       = slotMachine.betAmount;
        betText.text   = $"BET\n{FormatCoins(_lastBet)}";
    }

    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000L) return $"{v / 1_000_000_000L}B";
        if (v >= 1_000_000L)     return $"{v / 1_000_000L}M";
        if (v >= 1_000L)         return $"{v / 1_000L}K";
        return v.ToString();
    }
}
