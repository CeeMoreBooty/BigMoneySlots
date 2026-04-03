using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Builds the complete in-game UI at runtime so no manual Inspector wiring is
/// needed.  Finds existing SlotUI, JackpotUI, SettingsUI, and HUDManager
/// MonoBehaviours and fills in every public field they expose.
///
/// Layout (1920×1080 reference resolution):
///   ┌─ TOP BAR ─ balance | bet | jackpot ticker ─────────────────────────────┐
///   │  REEL PANEL  [sym0] [sym1] [sym2]                                       │
///   │  TOAST / result text  (centred)                                         │
///   │  JACKPOT TIER ROW  MINI/MINOR/MAJOR/GRAND/MEGA                          │
///   └─ CONTROL BAR  [−] BET [+]  [SETTINGS]  [AUTO SPIN]  [ S P I N ] ───────┘
///
/// Keyboard shortcut: Space = Spin (also guarded by FraudPrevention).
/// </summary>
public class UIBuilder : MonoBehaviour
{
    // ── Colour palette ────────────────────────────────────────────────────────
    private static readonly Color COL_DARK    = new Color(0.04f, 0.01f, 0.07f, 0.97f);
    private static readonly Color COL_PANEL   = new Color(0.10f, 0.04f, 0.16f, 0.90f);
    private static readonly Color COL_GOLD    = new Color(1.00f, 0.84f, 0.00f, 1.00f);
    private static readonly Color COL_GREEN   = new Color(0.08f, 0.70f, 0.08f, 1.00f);
    private static readonly Color COL_ORANGE  = new Color(0.80f, 0.40f, 0.00f, 1.00f);
    private static readonly Color COL_PURPLE  = new Color(0.35f, 0.10f, 0.55f, 1.00f);
    private static readonly Color COL_GREY    = new Color(0.22f, 0.22f, 0.22f, 1.00f);

    // ── Runtime references ────────────────────────────────────────────────────
    private SlotUI      _slotUI;
    private JackpotUI   _jackpotUI;
    private SlotMachine _slotMachine;
    private SettingsUI  _settingsUI;
    private HUDManager  _hudManager;

    // Reel symbol text boxes (updated each frame)
    private TextMeshProUGUI[] _reelLabels;

    // Settings overlay panel (built here, handed to SettingsUI)
    private GameObject _settingsPanel;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        _slotUI      = FindObjectOfType<SlotUI>();
        _jackpotUI   = FindObjectOfType<JackpotUI>();
        _slotMachine = FindObjectOfType<SlotMachine>();
        _settingsUI  = FindObjectOfType<SettingsUI>();
        _hudManager  = FindObjectOfType<HUDManager>();

        EnsureEventSystem();
        Canvas canvas = BuildCanvas();

        BuildBackground(canvas);
        BuildTopBar(canvas);
        BuildReelPanel(canvas);
        BuildResultAndToast(canvas);
        BuildJackpotTierRow(canvas);
        BuildSettingsPanel(canvas);    // build first so control bar can wire the button
        BuildControlBar(canvas);

        WireSlotUIEvents();

        Debug.Log("[UIBuilder] UI construction complete.");
    }

    private void Update()
    {
        RefreshReelLabels();

        // Keyboard shortcut: Space = Spin
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool ok = FraudPrevention.Instance == null ||
                      FraudPrevention.Instance.AllowSpin();
            if (ok) _slotMachine?.Spin();
        }
    }

    // ── EventSystem ───────────────────────────────────────────────────────────
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
        var bg = MakeRect(canvas.transform, "BG",
                          Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.AddComponent<Image>().color = COL_DARK;
    }

    // ── Top bar ───────────────────────────────────────────────────────────────
    private void BuildTopBar(Canvas canvas)
    {
        var bar = MakeRect(canvas.transform, "TopBar",
                           new Vector2(0, 1), new Vector2(1, 1),
                           new Vector2(0, -72), Vector2.zero);
        bar.AddComponent<Image>().color = COL_PANEL;

        // Balance (left)
        var balTxt = MakeTMP(bar.transform, "BalanceTxt",
                             "  \U0001f4b0  Balance: —", 26,
                             TextAlignmentOptions.Left,
                             new Vector2(0, 0), new Vector2(0.40f, 1));
        balTxt.color = COL_GOLD;

        // Bet (center)
        var betTxt = MakeTMP(bar.transform, "BetTxt",
                             "Bet: 100", 22,
                             TextAlignmentOptions.Center,
                             new Vector2(0.38f, 0), new Vector2(0.62f, 1));
        betTxt.color = Color.white;

        // Jackpot (right)
        var jpTxt = MakeTMP(bar.transform, "JackpotTxt",
                            "\U0001f3b0  JACKPOT: —  ", 24,
                            TextAlignmentOptions.Right,
                            new Vector2(0.60f, 0), new Vector2(1, 1));
        jpTxt.color = COL_GOLD;

        // Wire to SlotUI
        if (_slotUI != null)
        {
            _slotUI.balanceText = balTxt;
            _slotUI.betText     = betTxt;
            _slotUI.jackpotText = jpTxt;
        }
    }

    // ── Reel panel ────────────────────────────────────────────────────────────
    private void BuildReelPanel(Canvas canvas)
    {
        var panel = MakeRect(canvas.transform, "ReelPanel",
                             new Vector2(0.10f, 0.46f), new Vector2(0.90f, 0.76f),
                             Vector2.zero, Vector2.zero);
        panel.AddComponent<Image>().color = COL_PANEL;

        _reelLabels = new TextMeshProUGUI[3];
        string[] names = { "CHERRY", "BELL", "BAR" };

        for (int i = 0; i < 3; i++)
        {
            float x0 = i / 3f, x1 = (i + 1) / 3f;
            var box = MakeRect(panel.transform, "Reel" + i,
                               new Vector2(x0 + 0.01f, 0.06f),
                               new Vector2(x1 - 0.01f, 0.94f),
                               Vector2.zero, Vector2.zero);
            box.AddComponent<Image>().color = COL_PURPLE;

            _reelLabels[i] = MakeTMP(box.transform, "Sym",
                                     names[i], 38,
                                     TextAlignmentOptions.Center,
                                     Vector2.zero, Vector2.one);
            _reelLabels[i].color     = COL_GOLD;
            _reelLabels[i].fontStyle = FontStyles.Bold;
        }
    }

    private void RefreshReelLabels()
    {
        if (_reelLabels == null || _slotMachine == null) return;
        var reels = _slotMachine.reels;
        if (reels == null) return;
        for (int i = 0; i < _reelLabels.Length && i < reels.Length; i++)
        {
            if (_reelLabels[i] == null || reels[i] == null) continue;
            var sym = reels[i].CurrentSymbol;
            if (sym != null) _reelLabels[i].text = sym.symbolName.ToUpper();
        }
    }

    // ── Result + toast ────────────────────────────────────────────────────────
    private void BuildResultAndToast(Canvas canvas)
    {
        // Spin result text (wired into SlotUI)
        var resultTxt = MakeTMP(canvas.transform, "ResultTxt",
                                "", 40,
                                TextAlignmentOptions.Center,
                                new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.46f));
        resultTxt.color     = COL_GOLD;
        resultTxt.fontStyle = FontStyles.Bold;

        if (_slotUI != null) _slotUI.resultText = resultTxt;

        // HUD toast (milestone / tier-up / session rewards)
        var toastTxt = MakeTMP(canvas.transform, "ToastTxt",
                               "", 28,
                               TextAlignmentOptions.Center,
                               new Vector2(0.05f, 0.79f), new Vector2(0.95f, 0.87f));
        toastTxt.color = new Color(0.2f, 1f, 0.4f, 1f);
        toastTxt.fontStyle = FontStyles.Bold;

        if (_hudManager != null) _hudManager.toastText = toastTxt;
    }

    // ── Jackpot tier row ──────────────────────────────────────────────────────
    private void BuildJackpotTierRow(Canvas canvas)
    {
        var row = MakeRect(canvas.transform, "JackpotRow",
                           new Vector2(0, 0.26f), new Vector2(1, 0.36f),
                           Vector2.zero, Vector2.zero);
        row.AddComponent<Image>().color = new Color(0.06f, 0.02f, 0.10f, 0.90f);

        string[] labels = { "MINI", "MINOR", "MAJOR", "GRAND", "MEGA" };
        var tiers = new[]
        {
            ProgressiveJackpot.JackpotTier.Mini,
            ProgressiveJackpot.JackpotTier.Minor,
            ProgressiveJackpot.JackpotTier.Major,
            ProgressiveJackpot.JackpotTier.Grand,
            ProgressiveJackpot.JackpotTier.Mega,
        };
        var tierColors = new[]
        {
            new Color(0.7f, 0.7f, 0.7f, 1f),
            new Color(0.0f, 0.8f, 0.4f, 1f),
            new Color(0.0f, 0.6f, 1.0f, 1f),
            new Color(0.8f, 0.2f, 0.8f, 1f),
            COL_GOLD,
        };

        var displays = new List<JackpotUI.TierDisplay>();
        for (int i = 0; i < 5; i++)
        {
            float x0 = i / 5f, x1 = (i + 1) / 5f;
            var cell = MakeRect(row.transform, "Tier_" + labels[i],
                                new Vector2(x0 + 0.003f, 0.04f),
                                new Vector2(x1 - 0.003f, 0.96f),
                                Vector2.zero, Vector2.zero);
            cell.AddComponent<Image>().color = COL_PANEL;

            var txt = MakeTMP(cell.transform, "Pool",
                              labels[i] + "\n—", 18,
                              TextAlignmentOptions.Center,
                              Vector2.zero, Vector2.one);
            txt.color     = tierColors[i];
            txt.fontStyle = FontStyles.Bold;

            displays.Add(new JackpotUI.TierDisplay
            {
                tier     = tiers[i],
                poolText = txt,
            });
        }

        if (_jackpotUI != null)
            _jackpotUI.tierDisplays = displays.ToArray();
    }

    // ── Settings panel (built here, handed to SettingsUI) ─────────────────────
    private void BuildSettingsPanel(Canvas canvas)
    {
        _settingsPanel = MakeRect(canvas.transform, "SettingsPanel",
                                   new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f),
                                   Vector2.zero, Vector2.zero);
        _settingsPanel.AddComponent<Image>().color = new Color(0.08f, 0.03f, 0.12f, 0.97f);
        _settingsPanel.SetActive(false);

        // Title
        var title = MakeTMP(_settingsPanel.transform, "Title",
                            "⚙  SETTINGS", 32,
                            TextAlignmentOptions.Center,
                            new Vector2(0, 0.88f), new Vector2(1, 1));
        title.color = COL_GOLD;

        // Sound toggle
        BuildToggleRow(_settingsPanel.transform, "Sound", "🔊  Sound",
                       0.72f, 0.82f,
                       PlayerPrefs.GetInt("settings_sound", 1) == 1,
                       isOn =>
                       {
                           AudioListener.volume = isOn ? 1f : 0f;
                           PlayerPrefs.SetInt("settings_sound", isOn ? 1 : 0);
                           PlayerPrefs.Save();
                       });

        // Auto-spin speed buttons
        var speedLbl = MakeTMP(_settingsPanel.transform, "SpeedLbl",
                               "Auto Speed:  Normal (1 s)", 22,
                               TextAlignmentOptions.Left,
                               new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.70f));
        speedLbl.color = Color.white;

        var speedNorm = MakeButton(_settingsPanel.transform, "SpeedNorm", "Normal",
                                   new Vector2(0.05f, 0.50f), new Vector2(0.35f, 0.59f), COL_GREY);
        speedNorm.onClick.AddListener(() => ApplyAutoSpinSpeed(1.0f, "Normal (1 s)", speedLbl));

        var speedFast = MakeButton(_settingsPanel.transform, "SpeedFast", "Fast",
                                   new Vector2(0.38f, 0.50f), new Vector2(0.63f, 0.59f), COL_GREY);
        speedFast.onClick.AddListener(() => ApplyAutoSpinSpeed(0.5f, "Fast (0.5 s)", speedLbl));

        var speedMax = MakeButton(_settingsPanel.transform, "SpeedMax", "Max",
                                  new Vector2(0.66f, 0.50f), new Vector2(0.95f, 0.59f), COL_GREY);
        speedMax.onClick.AddListener(() => ApplyAutoSpinSpeed(0.1f, "Max (0.1 s)", speedLbl));

        // Version info
        var verTxt = MakeTMP(_settingsPanel.transform, "Version",
                             "v" + Application.version + "  |  Unity " + Application.unityVersion,
                             16, TextAlignmentOptions.Center,
                             new Vector2(0, 0.02f), new Vector2(1, 0.10f));
        verTxt.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Close button
        var closeBtn = MakeButton(_settingsPanel.transform, "CloseBtn", "✕  Close",
                                  new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.22f),
                                  new Color(0.5f, 0.0f, 0.0f, 1f));
        closeBtn.onClick.AddListener(() => _settingsPanel.SetActive(false));

        // Hand panel to SettingsUI so its Toggle() / Show() / Hide() work
        if (_settingsUI != null) _settingsUI.panel = _settingsPanel;
    }

    private void ApplyAutoSpinSpeed(float delay, string label, TextMeshProUGUI lbl)
    {
        if (_slotMachine != null) _slotMachine.autoSpinDelay = delay;
        if (lbl != null) lbl.text = "Auto Speed:  " + label;
    }

    private static void BuildToggleRow(Transform parent, string id, string labelText,
                                        float yMin, float yMax, bool initial,
                                        System.Action<bool> onChanged)
    {
        var lbl = MakeTMP(parent, id + "Lbl", labelText, 22,
                          TextAlignmentOptions.Left,
                          new Vector2(0.05f, yMin), new Vector2(0.75f, yMax));
        lbl.color = Color.white;

        // Simple on/off toggle button
        bool state = initial;
        var btn = MakeButton(parent, id + "Btn", state ? "ON" : "OFF",
                              new Vector2(0.78f, yMin + 0.01f),
                              new Vector2(0.95f, yMax - 0.01f),
                              state ? COL_GREEN : COL_GREY);
        TextMeshProUGUI btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
        Image           btnImg = btn.GetComponent<Image>();
        btn.onClick.AddListener(() =>
        {
            state = !state;
            if (btnLbl != null) btnLbl.text = state ? "ON" : "OFF";
            if (btnImg  != null) btnImg.color = state ? COL_GREEN : COL_GREY;
            onChanged(state);
        });
    }

    // ── Control bar ───────────────────────────────────────────────────────────
    private void BuildControlBar(Canvas canvas)
    {
        var bar = MakeRect(canvas.transform, "ControlBar",
                           Vector2.zero, new Vector2(1, 0.23f),
                           new Vector2(0, 6), Vector2.zero);
        bar.AddComponent<Image>().color = COL_PANEL;

        // BET −
        var betDec = MakeButton(bar.transform, "BetDec", "−",
                                new Vector2(0.01f, 0.08f), new Vector2(0.10f, 0.92f),
                                COL_PURPLE);
        betDec.onClick.AddListener(() =>
        {
            if (_slotMachine != null)
                _slotMachine.betAmount = System.Math.Max(100, _slotMachine.betAmount / 2);
        });

        // BET display
        var betDisplay = MakeTMP(bar.transform, "BetDisplay",
                                 "BET\n100", 20,
                                 TextAlignmentOptions.Center,
                                 new Vector2(0.10f, 0.04f), new Vector2(0.24f, 0.96f));
        betDisplay.color = Color.white;

        // BET +
        var betInc = MakeButton(bar.transform, "BetInc", "+",
                                new Vector2(0.24f, 0.08f), new Vector2(0.33f, 0.92f),
                                COL_PURPLE);
        betInc.onClick.AddListener(() =>
        {
            if (_slotMachine != null)
                _slotMachine.betAmount = System.Math.Min(10_000_000, _slotMachine.betAmount * 2);
        });

        // SETTINGS
        var settingsBtn = MakeButton(bar.transform, "SettingsBtn", "⚙",
                                     new Vector2(0.35f, 0.08f), new Vector2(0.44f, 0.92f),
                                     COL_GREY);
        settingsBtn.onClick.AddListener(() =>
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        });

        // AUTO SPIN
        var autoBtn = MakeButton(bar.transform, "AutoBtn", "AUTO",
                                  new Vector2(0.46f, 0.08f), new Vector2(0.62f, 0.92f),
                                  COL_ORANGE);
        var autoBtnLabel = autoBtn.GetComponentInChildren<TextMeshProUGUI>();
        autoBtn.onClick.AddListener(() => _slotMachine?.ToggleAutoSpin());

        // SPIN  (guarded by FraudPrevention)
        var spinBtn = MakeButton(bar.transform, "SpinBtn",
                                  "\U0001f3b0  S P I N",
                                  new Vector2(0.64f, 0.04f), new Vector2(0.99f, 0.96f),
                                  COL_GREEN);
        spinBtn.onClick.AddListener(() =>
        {
            bool ok = FraudPrevention.Instance == null ||
                      FraudPrevention.Instance.AllowSpin();
            if (ok) _slotMachine?.Spin();
        });

        // Wire SlotUI button refs so HandleAutoSpinChanged() can toggle text+interactable
        if (_slotUI != null)
        {
            _slotUI.spinButton         = spinBtn.GetComponent<Button>();
            _slotUI.autoSpinButton     = autoBtn.GetComponent<Button>();
            _slotUI.autoSpinButtonText = autoBtnLabel;
        }

        // Bet display sync via a small helper component
        var sync = bar.AddComponent<BetSync>();
        sync.betDisplay  = betDisplay;
        sync.slotMachine = _slotMachine;
    }

    // ── Wire SlotUI event handlers after scene Start() ────────────────────────
    private void WireSlotUIEvents()
    {
        // SlotUI.Awake() ran with null buttons — we must add listeners ourselves.
        if (_slotUI == null || _slotMachine == null) return;

        // OnSpinComplete → update resultText (already done via event in SlotUI.OnEnable)
        // OnAutoSpinChanged → update button label / interactable (also in SlotUI.OnEnable)
        // Both subscriptions happen in OnEnable which fires *before* Start, so they are
        // already active; our late-set text refs will be used correctly.
    }

    // ── Factory helpers ───────────────────────────────────────────────────────
    private static GameObject MakeRect(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    private static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
        float size, TextAlignmentOptions align,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = MakeRect(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = text;
        tmp.fontSize          = size;
        tmp.alignment         = align;
        tmp.enableWordWrapping = false;
        tmp.overflowMode      = TextOverflowModes.Ellipsis;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bg)
    {
        var go = MakeRect(parent, name, anchorMin, anchorMax,
                          new Vector2(2, 2), new Vector2(-2, -2));
        var img   = go.AddComponent<Image>();
        img.color = bg;
        var btn   = go.AddComponent<Button>();
        var cb    = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Min(bg.r + 0.18f, 1f),
            Mathf.Min(bg.g + 0.18f, 1f),
            Mathf.Min(bg.b + 0.18f, 1f), 1f);
        cb.pressedColor = new Color(bg.r * 0.65f, bg.g * 0.65f, bg.b * 0.65f, 1f);
        btn.colors = cb;

        var lblGO = MakeRect(go.transform, "Label",
                             Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var tmp           = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 24;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = Color.white;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        return btn;
    }
}

// ── Tiny helper: keeps BET display in sync every frame ────────────────────────
internal class BetSync : MonoBehaviour
{
    public TextMeshProUGUI betDisplay;
    public SlotMachine     slotMachine;
    private long _last = -1;

    private void Update()
    {
        if (slotMachine == null || betDisplay == null) return;
        if (slotMachine.betAmount == _last) return;
        _last = slotMachine.betAmount;
        betDisplay.text = "BET\n" + FormatCoins(_last);
    }

    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000L) return v / 1_000_000_000L + "B";
        if (v >= 1_000_000L)     return v / 1_000_000L + "M";
        if (v >= 1_000L)         return v / 1_000L + "K";
        return v.ToString();
    }
}
