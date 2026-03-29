#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tool that builds the entire Big Money Slots UI hierarchy in the
/// currently open scene.
///
/// Usage:  Unity menu ▶  BigMoneySlots ▶ Setup Full UI
///
/// What it creates (idempotent – skips objects that already exist):
///   • EventSystem + StandaloneInputModule
///   • Systems (SlotMachine, PlayerEconomy, ProgressiveJackpot, SlotGameLoader)
///   • HUD Canvas  (CasinoHUD – top bar with coins, gems, level, jackpot ticker)
///   • Game Canvas (SlotGameSceneController)
///       ├ AccentBar
///       ├ GameInfoPanel  (game name, theme)
///       ├ JackpotLabel
///       ├ ReelGrid       (5 reels × 3 rows of TMP_Text)
///       ├ BetControlsPanel (Bet+/Bet–/BetMax/Bet1/AutoSpin/SpinButton)
///       ├ HUDLabels      (Coins, Win, Payline, RTP)
///       ├ WinCelebrationUI overlay
///       └ BackButton
///   • BottomNavBar Canvas (Home/Games/Jackpots/Rewards/Profile tabs)
/// </summary>
public static class UISetupTool
{
    // ── colours ───────────────────────────────────────────────────────────
    static readonly Color BgDark       = new Color(0.07f, 0.05f, 0.14f, 1f);
    static readonly Color Gold         = new Color(1.00f, 0.80f, 0.00f, 1f);
    static readonly Color DarkPanel    = new Color(0.10f, 0.07f, 0.20f, 0.95f);
    static readonly Color ButtonNormal = new Color(0.14f, 0.08f, 0.28f, 1f);
    static readonly Color SpinColor    = new Color(1.00f, 0.55f, 0.00f, 1f);
    static readonly Color HUDBar       = new Color(0.06f, 0.04f, 0.12f, 0.97f);
    static readonly Color NavBar       = new Color(0.06f, 0.04f, 0.12f, 0.97f);

    // ── entry point ───────────────────────────────────────────────────────
    [MenuItem("BigMoneySlots/Setup Full UI")]
    public static void SetupFullUI()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("BigMoneySlots – Setup Full UI");

        EnsureEventSystem();
        GameObject systems = EnsureSystems();

        GameObject hudCanvas  = BuildHUDCanvas(systems);
        GameObject gameCanvas = BuildGameCanvas(systems);
        BuildBottomNavBar();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = gameCanvas;
        Debug.Log("[BigMoneySlots] ✅ Full UI setup complete – press PLAY to test.");
    }

    // ── EventSystem ───────────────────────────────────────────────────────
    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem");
        RegisterUndo(go);
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    // ── Systems ───────────────────────────────────────────────────────────
    static GameObject EnsureSystems()
    {
        GameObject sys = GameObject.Find("Systems");
        if (sys == null)
        {
            sys = new GameObject("Systems");
            RegisterUndo(sys);
        }

        AddIfMissing<PlayerEconomy>(sys);
        AddIfMissing<SlotMachine>(sys);
        AddIfMissing<ProgressiveJackpot>(sys);
        AddIfMissing<SlotGameLoader>(sys);
        return sys;
    }

    // ─────────────────────────────────────────────────────────────────────
    // HUD CANVAS
    // ─────────────────────────────────────────────────────────────────────
    static GameObject BuildHUDCanvas(GameObject systems)
    {
        if (GameObject.Find("HUDCanvas") != null)
        {
            Debug.Log("[BigMoneySlots] HUDCanvas already exists – skipped.");
            return GameObject.Find("HUDCanvas");
        }

        // ── root canvas ──────────────────────────────────────────────────
        GameObject root = MakeCanvas("HUDCanvas", 100, sortOrder: 10);

        // ── HUD bar (top strip) ──────────────────────────────────────────
        GameObject bar = MakePanel(root, "HUDBar",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -50f), new Vector2(0f, 0f),
            100f, HUDBar);

        // coins
        GameObject coinIcon   = MakeImage(bar, "CoinIcon",
            AnchorPreset.MiddleLeft, new Vector2(30f, 0f), new Vector2(40f, 40f), Gold);
        GameObject coinsLabel = MakeTMP(bar, "CoinsLabel",
            AnchorPreset.MiddleLeft, new Vector2(80f, 0f), new Vector2(140f, 40f),
            "0", 20f, TextAlignmentOptions.Left, Gold);

        // gems
        GameObject gemIcon   = MakeImage(bar, "GemIcon",
            AnchorPreset.MiddleLeft, new Vector2(240f, 0f), new Vector2(40f, 40f),
            new Color(0.2f, 0.8f, 1f));
        GameObject gemsLabel = MakeTMP(bar, "GemsLabel",
            AnchorPreset.MiddleLeft, new Vector2(290f, 0f), new Vector2(100f, 40f),
            "0", 18f, TextAlignmentOptions.Left, new Color(0.2f, 0.8f, 1f));

        // player info (right side)
        GameObject nameLabel  = MakeTMP(bar, "PlayerNameLabel",
            AnchorPreset.MiddleRight, new Vector2(-200f, 8f), new Vector2(160f, 28f),
            "Player", 16f, TextAlignmentOptions.Right, Color.white);
        GameObject levelLabel = MakeTMP(bar, "PlayerLevelLabel",
            AnchorPreset.MiddleRight, new Vector2(-200f, -14f), new Vector2(160f, 24f),
            "Lv.1", 14f, TextAlignmentOptions.Right, Gold);
        GameObject xpBar      = MakeSlider(bar, "XPBar",
            AnchorPreset.MiddleRight, new Vector2(-50f, 0f), new Vector2(120f, 8f));

        // jackpot panel
        GameObject jpPanel = MakePanel(bar, "JackpotPanel",
            AnchorPreset.MiddleCenter, Vector2.zero, new Vector2(220f, 44f), Gold);
        SetPanelAlpha(jpPanel, 0.15f);
        GameObject jpLabel = MakeTMP(jpPanel, "JackpotLabel",
            AnchorPreset.Stretch, Vector2.zero, Vector2.zero,
            "🎰 JACKPOT  0", 17f, TextAlignmentOptions.Center, Gold);

        // buttons
        GameObject addCoinsBtn    = MakeButton(bar, "AddCoinsButton",
            AnchorPreset.MiddleLeft, new Vector2(410f, 0f), new Vector2(90f, 40f),
            "+ COINS", 15f, Gold, ButtonNormal);
        GameObject dailyBonusBtn  = MakeButton(bar, "DailyBonusButton",
            AnchorPreset.MiddleLeft, new Vector2(515f, 0f), new Vector2(110f, 40f),
            "🎁 BONUS", 15f, Gold, ButtonNormal);
        GameObject dailyBadge     = MakeBadge(dailyBonusBtn, "DailyBonusBadge");
        GameObject settingsBtn    = MakeButton(bar, "SettingsButton",
            AnchorPreset.MiddleRight, new Vector2(-40f, 0f), new Vector2(44f, 44f),
            "⚙", 22f, Color.white, ButtonNormal);

        // ── wire CasinoHUD ───────────────────────────────────────────────
        CasinoHUD hud = root.AddComponent<CasinoHUD>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("coinsLabel")      .objectReferenceValue = coinsLabel.GetComponent<TMP_Text>();
        so.FindProperty("gemsLabel")       .objectReferenceValue = gemsLabel.GetComponent<TMP_Text>();
        so.FindProperty("coinIcon")        .objectReferenceValue = coinIcon.GetComponent<Image>();
        so.FindProperty("gemIcon")         .objectReferenceValue = gemIcon.GetComponent<Image>();
        so.FindProperty("playerNameLabel") .objectReferenceValue = nameLabel.GetComponent<TMP_Text>();
        so.FindProperty("playerLevelLabel").objectReferenceValue = levelLabel.GetComponent<TMP_Text>();
        so.FindProperty("xpBar")           .objectReferenceValue = xpBar.GetComponent<Slider>();
        so.FindProperty("jackpotLabel")    .objectReferenceValue = jpLabel.GetComponent<TMP_Text>();
        so.FindProperty("jackpotPanel")    .objectReferenceValue = jpPanel;
        so.FindProperty("addCoinsButton")  .objectReferenceValue = addCoinsBtn.GetComponent<Button>();
        so.FindProperty("dailyBonusButton").objectReferenceValue = dailyBonusBtn.GetComponent<Button>();
        so.FindProperty("dailyBonusBadge") .objectReferenceValue = dailyBadge;
        so.FindProperty("settingsButton")  .objectReferenceValue = settingsBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        return root;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GAME CANVAS  (SlotGameSceneController)
    // ─────────────────────────────────────────────────────────────────────
    static GameObject BuildGameCanvas(GameObject systems)
    {
        if (GameObject.Find("GameCanvas") != null)
        {
            Debug.Log("[BigMoneySlots] GameCanvas already exists – skipped.");
            return GameObject.Find("GameCanvas");
        }

        // ── root canvas ──────────────────────────────────────────────────
        GameObject root = MakeCanvas("GameCanvas", 200, sortOrder: 5);

        // dark background
        Image bg = root.GetComponent<Image>();
        if (bg == null) bg = root.AddComponent<Image>();
        bg.color = BgDark;

        // ── accent bar (top colour stripe) ───────────────────────────────
        GameObject accentBar = MakePanel(root, "AccentBar",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -6f), new Vector2(0f, 0f), 12f, Gold);

        // ── game info panel ───────────────────────────────────────────────
        GameObject infoPanel = MakePanel(root, "GameInfoPanel",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -100f), new Vector2(0f, -12f), 88f, DarkPanel);

        GameObject gameNameLabel  = MakeTMP(infoPanel, "GameNameLabel",
            AnchorPreset.MiddleLeft, new Vector2(20f, 12f), new Vector2(300f, 38f),
            "BIG MONEY SLOTS", 26f, TextAlignmentOptions.Left, Gold);
        GameObject gameThemeLabel = MakeTMP(infoPanel, "GameThemeLabel",
            AnchorPreset.MiddleLeft, new Vector2(20f, -14f), new Vector2(300f, 26f),
            "Classic", 16f, TextAlignmentOptions.Left, Color.white);
        GameObject paylineLabel   = MakeTMP(infoPanel, "PaylineLabel",
            AnchorPreset.MiddleRight, new Vector2(-130f, 8f), new Vector2(120f, 26f),
            "25 LINES", 16f, TextAlignmentOptions.Right, Color.white);
        GameObject rtpLabel       = MakeTMP(infoPanel, "RTPLabel",
            AnchorPreset.MiddleRight, new Vector2(-130f, -14f), new Vector2(120f, 24f),
            "RTP 96%", 14f, TextAlignmentOptions.Right, new Color(0.6f, 1f, 0.6f));

        // ── jackpot label ─────────────────────────────────────────────────
        GameObject jackpotLabel = MakeTMP(root, "JackpotLabel",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -138f), new Vector2(0f, -100f),
            "🏆  0", 22f, TextAlignmentOptions.Center, Gold);

        // ── reel frame panel ──────────────────────────────────────────────
        //   Placed in the vertical center of the screen (anchored mid-stretch)
        GameObject reelFrame = MakePanel(root, "ReelFrame",
            new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.76f),
            Vector2.zero, Vector2.zero, 0f, new Color(0.05f, 0.03f, 0.12f, 1f));
        // add thin border via outline image
        Image reelFrameImg = reelFrame.GetComponent<Image>();
        reelFrameImg.color = new Color(0.08f, 0.05f, 0.18f);

        // ── 5 × 3 reel grid ───────────────────────────────────────────────
        // Each reel is a vertical column; each cell is a TMP_Text showing a symbol.
        TMP_Text[][] allReelRows = new TMP_Text[5][];
        string[]     reelNames  = { "Reel0", "Reel1", "Reel2", "Reel3", "Reel4" };

        for (int r = 0; r < 5; r++)
        {
            // reel column container
            GameObject reelGO = new GameObject(reelNames[r]);
            RegisterUndo(reelGO);
            reelGO.transform.SetParent(reelFrame.transform, false);

            RectTransform reelRT = reelGO.AddComponent<RectTransform>();
            float xMin = r / 5f;
            float xMax = (r + 1) / 5f;
            reelRT.anchorMin = new Vector2(xMin, 0f);
            reelRT.anchorMax = new Vector2(xMax, 1f);
            reelRT.offsetMin = new Vector2(4f, 4f);
            reelRT.offsetMax = new Vector2(-4f, -4f);

            // thin separator line (not last reel)
            if (r < 4)
            {
                GameObject sep = new GameObject("Separator");
                RegisterUndo(sep);
                sep.transform.SetParent(reelGO.transform, false);
                RectTransform sepRT = sep.AddComponent<RectTransform>();
                sepRT.anchorMin = new Vector2(1f, 0f);
                sepRT.anchorMax = new Vector2(1f, 1f);
                sepRT.offsetMin = new Vector2(0f, 0f);
                sepRT.offsetMax = new Vector2(2f, 0f);
                Image sepImg = sep.AddComponent<Image>();
                sepImg.color = new Color(0.3f, 0.2f, 0.5f, 0.5f);
            }

            allReelRows[r] = new TMP_Text[3];
            for (int row = 0; row < 3; row++)
            {
                GameObject cell = new GameObject($"Row{row}");
                RegisterUndo(cell);
                cell.transform.SetParent(reelGO.transform, false);

                RectTransform cellRT = cell.AddComponent<RectTransform>();
                float yMin = (2 - row) / 3f;
                float yMax = (3 - row) / 3f;
                cellRT.anchorMin = new Vector2(0f, yMin);
                cellRT.anchorMax = new Vector2(1f, yMax);
                cellRT.offsetMin = new Vector2(4f, 4f);
                cellRT.offsetMax = new Vector2(-4f, -4f);

                // highlight middle row
                if (row == 1)
                {
                    Image cellBg = cell.AddComponent<Image>();
                    cellBg.color = new Color(1f, 0.8f, 0f, 0.07f);
                }

                TMP_Text sym = cell.AddComponent<TextMeshProUGUI>();
                sym.text      = "🍒";
                sym.fontSize  = 42f;
                sym.alignment = TextAlignmentOptions.Center;
                sym.color     = Color.white;
                allReelRows[r][row] = sym;

                cell.AddComponent<CanvasRenderer>();
            }
        }

        // ── payline highlight (centre row) ────────────────────────────────
        GameObject paylineHighlight = MakePanel(reelFrame, "PaylineHighlight",
            new Vector2(0f, 0.333f), new Vector2(1f, 0.666f),
            Vector2.zero, Vector2.zero, 0f, new Color(1f, 0.9f, 0f, 0.06f));

        // ── bottom HUD labels ─────────────────────────────────────────────
        GameObject hudLabels = MakePanel(root, "HUDLabels",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 155f), new Vector2(0f, 225f), 70f, DarkPanel);

        GameObject coinsLabel = MakeTMP(hudLabels, "CoinsLabel",
            AnchorPreset.MiddleLeft, new Vector2(20f, 0f), new Vector2(180f, 40f),
            "💰 0", 22f, TextAlignmentOptions.Left, Gold);
        GameObject winLabel   = MakeTMP(hudLabels, "WinLabel",
            AnchorPreset.MiddleCenter, Vector2.zero, new Vector2(280f, 40f),
            "", 22f, TextAlignmentOptions.Center, new Color(0f, 1f, 0.5f));
        GameObject jackpotLbl = MakeTMP(hudLabels, "JackpotLabelHUD",
            AnchorPreset.MiddleRight, new Vector2(-20f, 0f), new Vector2(200f, 40f),
            "🏆 0", 18f, TextAlignmentOptions.Right, Gold);

        // ── bet controls panel ────────────────────────────────────────────
        GameObject betPanel = MakePanel(root, "BetControlsPanel",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 75f), new Vector2(0f, 155f), 80f, DarkPanel);

        // BET label
        GameObject betLabel = MakeTMP(betPanel, "BetLabel",
            AnchorPreset.MiddleLeft, new Vector2(18f, 0f), new Vector2(160f, 36f),
            "BET  1,000", 18f, TextAlignmentOptions.Left, Color.white);

        // Bet – / Bet + / BET 1 / MAX buttons
        GameObject betMinusBtn = MakeButton(betPanel, "BetMinusButton",
            AnchorPreset.MiddleLeft, new Vector2(190f, 0f), new Vector2(56f, 48f),
            "–", 26f, Color.white, ButtonNormal);
        GameObject betPlusBtn  = MakeButton(betPanel, "BetPlusButton",
            AnchorPreset.MiddleLeft, new Vector2(254f, 0f), new Vector2(56f, 48f),
            "+", 26f, Color.white, ButtonNormal);
        GameObject betOneBtn   = MakeButton(betPanel, "BetOneButton",
            AnchorPreset.MiddleLeft, new Vector2(320f, 0f), new Vector2(76f, 48f),
            "BET 1", 15f, Color.white, ButtonNormal);
        GameObject betMaxBtn   = MakeButton(betPanel, "BetMaxButton",
            AnchorPreset.MiddleLeft, new Vector2(406f, 0f), new Vector2(76f, 48f),
            "MAX", 16f, Gold, new Color(0.25f, 0.1f, 0.05f));

        // AUTO-SPIN
        GameObject autoSpinBtn   = MakeButton(betPanel, "AutoSpinButton",
            AnchorPreset.MiddleRight, new Vector2(-200f, 0f), new Vector2(90f, 48f),
            "AUTO", 16f, Color.white, ButtonNormal);
        GameObject autoSpinLabel = autoSpinBtn.transform.Find("Label")?.gameObject
                                   ?? autoSpinBtn;

        // ── SPIN BUTTON ───────────────────────────────────────────────────
        GameObject spinBtn = MakePanel(root, "SpinButton",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-80f, 10f), new Vector2(80f, 90f), 0f, SpinColor);

        // circular look via rounded corners (fake with border-radius image)
        Image spinImg = spinBtn.GetComponent<Image>();
        spinImg.color = SpinColor;

        Button spinBtnComp = spinBtn.AddComponent<Button>();
        ColorBlock spinColors = spinBtnComp.colors;
        spinColors.normalColor      = SpinColor;
        spinColors.highlightedColor = new Color(1f, 0.7f, 0.1f);
        spinColors.pressedColor     = new Color(0.8f, 0.4f, 0f);
        spinBtnComp.colors = spinColors;

        GameObject spinLabel = MakeTMP(spinBtn, "SpinLabel",
            AnchorPreset.Stretch, Vector2.zero, Vector2.zero,
            "SPIN", 28f, TextAlignmentOptions.Center, Color.white);
        spinLabel.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // ── back button ───────────────────────────────────────────────────
        GameObject backBtn = MakeButton(root, "BackButton",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(16f, -105f), new Vector2(16f, -55f),
            "◀ Back", 16f, Color.white, ButtonNormal);
        RectTransform backRT = backBtn.GetComponent<RectTransform>();
        backRT.sizeDelta = new Vector2(100f, 44f);

        // ── win celebration overlay ───────────────────────────────────────
        GameObject winOverlay = MakePanel(root, "WinCelebrationUI",
            AnchorPreset.Stretch, Vector2.zero, Vector2.zero, 0f,
            new Color(0f, 0f, 0f, 0.75f));
        winOverlay.SetActive(false);

        CanvasGroup winCG = winOverlay.AddComponent<CanvasGroup>();
        winCG.alpha = 0f;

        GameObject glowBg   = MakePanel(winOverlay, "GlowBackground",
            AnchorPreset.Stretch, Vector2.zero, Vector2.zero, 0f,
            new Color(0.4f, 0f, 1f, 0.7f));
        GameObject winType  = MakeTMP(winOverlay, "WinTypeLabel",
            AnchorPreset.MiddleCenter, new Vector2(0f, 80f), new Vector2(500f, 100f),
            "🎉 EPIC WIN!", 56f, TextAlignmentOptions.Center, Gold);
        winType.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        GameObject amtLabel = MakeTMP(winOverlay, "AmountLabel",
            AnchorPreset.MiddleCenter, new Vector2(0f, -10f), new Vector2(500f, 80f),
            "+0", 44f, TextAlignmentOptions.Center, Color.white);
        GameObject subLbl   = MakeTMP(winOverlay, "SubLabel",
            AnchorPreset.MiddleCenter, new Vector2(0f, -80f), new Vector2(400f, 50f),
            "", 24f, TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 1f));
        GameObject closeBtn = MakeButton(winOverlay, "CloseButton",
            AnchorPreset.MiddleCenter, new Vector2(0f, -160f), new Vector2(160f, 54f),
            "COLLECT", 20f, Color.white, ButtonNormal);

        // ── wire SlotGameSceneController ──────────────────────────────────
        SlotGameSceneController sgsc = root.AddComponent<SlotGameSceneController>();
        SerializedObject so = new SerializedObject(sgsc);

        so.FindProperty("gameNameLabel") .objectReferenceValue = gameNameLabel.GetComponent<TMP_Text>();
        so.FindProperty("gameThemeLabel").objectReferenceValue = gameThemeLabel.GetComponent<TMP_Text>();
        so.FindProperty("accentBar")     .objectReferenceValue = accentBar.GetComponent<Image>();
        so.FindProperty("paylineLabel")  .objectReferenceValue = paylineLabel.GetComponent<TMP_Text>();
        so.FindProperty("rtpLabel")      .objectReferenceValue = rtpLabel.GetComponent<TMP_Text>();
        so.FindProperty("jackpotLabel")  .objectReferenceValue = jackpotLabel.GetComponent<TMP_Text>();
        so.FindProperty("coinsLabel")    .objectReferenceValue = coinsLabel.GetComponent<TMP_Text>();
        so.FindProperty("winLabel")      .objectReferenceValue = winLabel.GetComponent<TMP_Text>();

        so.FindProperty("spinButton")    .objectReferenceValue = spinBtnComp;
        so.FindProperty("betPlusButton") .objectReferenceValue = betPlusBtn.GetComponent<Button>();
        so.FindProperty("betMinusButton").objectReferenceValue = betMinusBtn.GetComponent<Button>();
        so.FindProperty("betMaxButton")  .objectReferenceValue = betMaxBtn.GetComponent<Button>();
        so.FindProperty("betOneButton")  .objectReferenceValue = betOneBtn.GetComponent<Button>();
        so.FindProperty("autoSpinButton").objectReferenceValue = autoSpinBtn.GetComponent<Button>();
        so.FindProperty("betLabel")      .objectReferenceValue = betLabel.GetComponent<TMP_Text>();
        so.FindProperty("autoSpinLabel") .objectReferenceValue = autoSpinBtn.GetComponentInChildren<TMP_Text>();
        so.FindProperty("backButton")    .objectReferenceValue = backBtn.GetComponent<Button>();

        // reel rows (5 reels × 3 rows each)
        SetReelRows(so, "reel0Rows", allReelRows[0]);
        SetReelRows(so, "reel1Rows", allReelRows[1]);
        SetReelRows(so, "reel2Rows", allReelRows[2]);
        SetReelRows(so, "reel3Rows", allReelRows[3]);
        SetReelRows(so, "reel4Rows", allReelRows[4]);

        // wire SlotMachine reference from Systems
        SlotMachine sm = systems.GetComponent<SlotMachine>();
        if (sm != null)
            so.FindProperty("_slotMachine").objectReferenceValue = sm;

        // win celebration
        WinCelebrationUI winCelebComp = winOverlay.AddComponent<WinCelebrationUI>();
        SerializedObject wso = new SerializedObject(winCelebComp);
        wso.FindProperty("canvasGroup")    .objectReferenceValue = winCG;
        wso.FindProperty("glowBackground") .objectReferenceValue = glowBg.GetComponent<Image>();
        wso.FindProperty("winTypeLabel")   .objectReferenceValue = winType.GetComponent<TMP_Text>();
        wso.FindProperty("amountLabel")    .objectReferenceValue = amtLabel.GetComponent<TMP_Text>();
        wso.FindProperty("subLabel")       .objectReferenceValue = subLbl.GetComponent<TMP_Text>();
        wso.FindProperty("closeButton")    .objectReferenceValue = closeBtn.GetComponent<Button>();
        wso.ApplyModifiedProperties();

        so.FindProperty("winCelebration").objectReferenceValue = winCelebComp;
        so.ApplyModifiedProperties();

        return root;
    }

    // ─────────────────────────────────────────────────────────────────────
    // BOTTOM NAV BAR
    // ─────────────────────────────────────────────────────────────────────
    static void BuildBottomNavBar()
    {
        if (GameObject.Find("BottomNavBarCanvas") != null)
        {
            Debug.Log("[BigMoneySlots] BottomNavBarCanvas already exists – skipped.");
            return;
        }

        GameObject root = MakeCanvas("BottomNavBarCanvas", 80, sortOrder: 10);

        // nav bar strip (anchored bottom)
        GameObject bar = MakePanel(root, "NavBar",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 72f), 72f, NavBar);

        string[] tabNames  = { "Home", "Games", "Jackpots", "Rewards", "Profile" };
        string[] tabEmoji  = { "🏠", "🎰", "🏆", "🎁", "👤" };

        var tabEntryProp  = new string[] {
            "tabHome", "tabGames", "tabJackpots", "tabRewards", "tabProfile"
        };

        BottomNavBar navComp = bar.AddComponent<BottomNavBar>();
        SerializedObject nso = new SerializedObject(navComp);

        for (int i = 0; i < 5; i++)
        {
            float xMin = i / 5f;
            float xMax = (i + 1) / 5f;

            GameObject tab = new GameObject($"Tab_{tabNames[i]}");
            RegisterUndo(tab);
            tab.transform.SetParent(bar.transform, false);
            RectTransform tabRT = tab.AddComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(xMin, 0f);
            tabRT.anchorMax = new Vector2(xMax, 1f);
            tabRT.offsetMin = Vector2.zero;
            tabRT.offsetMax = Vector2.zero;

            Button btn = tab.AddComponent<Button>();

            // icon
            GameObject iconGO = MakeImage(tab, "Icon",
                AnchorPreset.MiddleCenter, new Vector2(0f, 12f), new Vector2(28f, 28f),
                new Color(0.55f, 0.52f, 0.65f));

            // label
            GameObject labelGO = MakeTMP(tab, "Label",
                AnchorPreset.MiddleCenter, new Vector2(0f, -14f), new Vector2(80f, 22f),
                tabNames[i], 12f, TextAlignmentOptions.Center,
                new Color(0.55f, 0.52f, 0.65f));

            // badge dot (notification)
            GameObject badge = MakeBadge(tab, "Badge");

            // fill TabEntry fields
            SerializedProperty entryProp = nso.FindProperty(tabEntryProp[i]);
            if (entryProp != null)
            {
                entryProp.FindPropertyRelative("button")    .objectReferenceValue = btn;
                entryProp.FindPropertyRelative("iconImage") .objectReferenceValue = iconGO.GetComponent<Image>();
                entryProp.FindPropertyRelative("label")     .objectReferenceValue = labelGO.GetComponent<TMP_Text>();
                entryProp.FindPropertyRelative("badge")     .objectReferenceValue = badge;
            }
        }
        nso.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPER FACTORIES
    // ─────────────────────────────────────────────────────────────────────

    // Preset shorthand for anchor min/max combos used repeatedly
    enum AnchorPreset
    {
        Stretch,
        MiddleCenter,
        MiddleLeft,
        MiddleRight,
        TopLeft,
        TopRight,
    }

    static GameObject MakeCanvas(string name, int referenceHeight, int sortOrder = 0)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, referenceHeight <= 200 ? 1920f : 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // Panel with configurable anchors
    static GameObject MakePanel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        float height, Color color)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin    = anchorMin;
        rt.anchorMax    = anchorMax;
        rt.anchoredPosition = Vector2.zero;

        // When anchors == (0,y)/(1,y) treat as horizontal stretch; use offset
        if (Mathf.Approximately(anchorMin.x, 0f) && Mathf.Approximately(anchorMax.x, 1f))
        {
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }
        else if (Mathf.Approximately(anchorMin.x, anchorMax.x) &&
                 Mathf.Approximately(anchorMin.y, anchorMax.y))
        {
            rt.anchoredPosition = offsetMin;
            rt.sizeDelta        = offsetMax - offsetMin;
        }
        else
        {
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    // AnchorPreset overload for panels
    static GameObject MakePanel(GameObject parent, string name,
        AnchorPreset preset, Vector2 position, Vector2 size,
        float height, Color color)
    {
        GetAnchorValues(preset, out Vector2 aMin, out Vector2 aMax);
        return MakePanelRaw(parent, name, aMin, aMax, position, size, color);
    }

    static GameObject MakePanelRaw(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color  = color;
        return go;
    }

    static GameObject MakeImage(GameObject parent, string name,
        AnchorPreset preset, Vector2 position, Vector2 size, Color color)
    {
        GetAnchorValues(preset, out Vector2 aMin, out Vector2 aMax);
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color  = color;
        return go;
    }

    static GameObject MakeTMP(GameObject parent, string name,
        AnchorPreset preset, Vector2 position, Vector2 size,
        string text, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        GetAnchorValues(preset, out Vector2 aMin, out Vector2 aMax);
        return MakeTMPRaw(parent, name, aMin, aMax, position, size, text, fontSize, alignment, color);
    }

    // Overload accepting explicit anchors (for top-strip labels)
    static GameObject MakeTMP(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 offsetMin, Vector2 offsetMax,
        string text, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        return MakeTMPRaw(parent, name, aMin, aMax, offsetMin, offsetMax - offsetMin,
                          text, fontSize, alignment, color, useOffsets: true,
                          offsetMin2: offsetMin, offsetMax2: offsetMax);
    }

    static GameObject MakeTMPRaw(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
        string text, float fontSize,
        TextAlignmentOptions alignment, Color color,
        bool useOffsets = false,
        Vector2 offsetMin2 = default, Vector2 offsetMax2 = default)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        if (useOffsets)
        {
            rt.offsetMin = offsetMin2;
            rt.offsetMax = offsetMax2;
        }
        else
        {
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
        }

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = alignment;
        tmp.color     = color;
        return go;
    }

    static GameObject MakeButton(GameObject parent, string name,
        AnchorPreset preset, Vector2 position, Vector2 size,
        string label, float fontSize, Color textColor, Color bgColor)
    {
        GetAnchorValues(preset, out Vector2 aMin, out Vector2 aMax);
        return MakeButtonRaw(parent, name, aMin, aMax, position, size, label, fontSize, textColor, bgColor);
    }

    static GameObject MakeButton(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
        string label, float fontSize, Color textColor, Color bgColor)
    {
        return MakeButtonRaw(parent, name, aMin, aMax, pos, size, label, fontSize, textColor, bgColor);
    }

    static GameObject MakeButtonRaw(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
        string label, float fontSize, Color textColor, Color bgColor)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color  = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(bgColor, Color.black, 0.25f);
        btn.colors          = cb;

        // label child
        GameObject labelGO = new GameObject("Label");
        RegisterUndo(labelGO);
        labelGO.transform.SetParent(go.transform, false);

        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = textColor;

        return go;
    }

    static GameObject MakeSlider(GameObject parent, string name,
        AnchorPreset preset, Vector2 position, Vector2 size)
    {
        GetAnchorValues(preset, out Vector2 aMin, out Vector2 aMax);
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // background
        GameObject bgGO = new GameObject("Background");
        RegisterUndo(bgGO);
        bgGO.transform.SetParent(go.transform, false);
        RectTransform bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.15f, 0.3f);

        // fill
        GameObject fillAreaGO = new GameObject("FillArea");
        RegisterUndo(fillAreaGO);
        fillAreaGO.transform.SetParent(go.transform, false);
        RectTransform fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        RegisterUndo(fillGO);
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0.5f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Gold;

        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.value    = 0f;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return go;
    }

    static GameObject MakeBadge(GameObject parent, string name)
    {
        GameObject go = new GameObject(name);
        RegisterUndo(go);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-4f, -4f);
        rt.sizeDelta = new Vector2(14f, 14f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.2f, 0.2f);
        go.SetActive(false);
        return go;
    }

    static void SetPanelAlpha(GameObject go, float alpha)
    {
        Image img = go.GetComponent<Image>();
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    static void SetReelRows(SerializedObject so, string propName, TMP_Text[] rows)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null) return;
        prop.arraySize = rows.Length;
        for (int i = 0; i < rows.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
    }

    static void GetAnchorValues(AnchorPreset preset,
        out Vector2 anchorMin, out Vector2 anchorMax)
    {
        switch (preset)
        {
            case AnchorPreset.Stretch:
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            case AnchorPreset.MiddleCenter:
                anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                return;
            case AnchorPreset.MiddleLeft:
                anchorMin = anchorMax = new Vector2(0f, 0.5f);
                return;
            case AnchorPreset.MiddleRight:
                anchorMin = anchorMax = new Vector2(1f, 0.5f);
                return;
            case AnchorPreset.TopLeft:
                anchorMin = anchorMax = new Vector2(0f, 1f);
                return;
            case AnchorPreset.TopRight:
                anchorMin = anchorMax = new Vector2(1f, 1f);
                return;
            default:
                anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                return;
        }
    }

    static T AddIfMissing<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    static void RegisterUndo(GameObject go)
    {
        Undo.RegisterCreatedObjectUndo(go, "BigMoneySlots UI Setup");
    }
}
#endif
