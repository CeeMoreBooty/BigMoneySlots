// Copyright (c) 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the Terms and Conditions / No-Refund Policy panel at first launch.
///
/// Usage:
///   • Place this component on any GameObject in Bootstrap or MainMenu.
///   • The panel is shown automatically on first launch (PlayerPrefs "tc_accepted" == 0).
///   • The ACCEPT button saves acceptance and dismisses the panel.
///   • The DECLINE button quits the application.
///
/// All UI is built at runtime — no Inspector prefab or Canvas needed.
/// </summary>
public class TermsAndConditionsUI : MonoBehaviour
{
    private const string KEY_ACCEPTED = "tc_accepted";
    private const int    CURRENT_VERSION = 1;   // increment when T&C change

    private GameObject _panel;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        int accepted = PlayerPrefs.GetInt(KEY_ACCEPTED, 0);
        if (accepted < CURRENT_VERSION)
            BuildAndShow();
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    private void BuildAndShow()
    {
        // Ensure Canvas + EventSystem exist
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("TCCanvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            cgo.AddComponent<UnityEngine.UI.CanvasScaler>();
            cgo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Panel background
        _panel = new GameObject("TC_Panel");
        _panel.transform.SetParent(canvas.transform, false);
        var panelRT       = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var bg = _panel.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.01f, 0.08f, 0.97f);

        // Scroll view
        var svGO = new GameObject("ScrollView");
        svGO.transform.SetParent(_panel.transform, false);
        var svRT       = svGO.AddComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0.05f, 0.18f);
        svRT.anchorMax = new Vector2(0.95f, 0.90f);
        svRT.offsetMin = Vector2.zero;
        svRT.offsetMax = Vector2.zero;
        var svImg = svGO.AddComponent<Image>();
        svImg.color = new Color(0.08f, 0.04f, 0.14f, 0.90f);
        var sv = svGO.AddComponent<ScrollRect>();
        sv.horizontal = false;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(svGO.transform, false);
        var contentRT       = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1);
        contentRT.offsetMin = new Vector2(10, 0);
        contentRT.offsetMax = new Vector2(-10, 0);
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sv.content = contentRT;

        var textGO = new GameObject("TC_Text");
        textGO.transform.SetParent(contentGO.transform, false);
        var textRT       = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(8, 8);
        textRT.offsetMax = new Vector2(-8, -8);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text         = GetTermsText();
        tmp.fontSize     = 20;
        tmp.color        = Color.white;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;

        // Title
        var titleGO = new GameObject("TC_Title");
        titleGO.transform.SetParent(_panel.transform, false);
        var titleRT       = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.91f);
        titleRT.anchorMax = new Vector2(1, 0.99f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleTMP     = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text    = "📜  Terms & Conditions — Big Money Slots";
        titleTMP.fontSize= 26;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color   = new Color(1f, 0.84f, 0f, 1f);

        // ACCEPT button
        var acceptBtn = MakeButton("ACCEPT & PLAY", new Vector2(0.55f, 0.03f), new Vector2(0.95f, 0.14f),
                                   new Color(0.08f, 0.70f, 0.08f, 1f), _panel.transform);
        acceptBtn.onClick.AddListener(OnAccept);

        // DECLINE button
        var declineBtn = MakeButton("DECLINE (Exit)", new Vector2(0.05f, 0.03f), new Vector2(0.45f, 0.14f),
                                    new Color(0.6f, 0.08f, 0.08f, 1f), _panel.transform);
        declineBtn.onClick.AddListener(OnDecline);
    }

    private Button MakeButton(string label, Vector2 anchorMin, Vector2 anchorMax,
                               Color bg, Transform parent)
    {
        var go = new GameObject(label + "_Btn");
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(4, 4);
        rt.offsetMax = new Vector2(-4, -4);
        go.AddComponent<Image>().color = bg;
        var btn  = go.AddComponent<Button>();
        var lblGO= new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt       = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var t = lblGO.AddComponent<TextMeshProUGUI>();
        t.text      = label;
        t.fontSize  = 22;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = Color.white;
        t.fontStyle = FontStyles.Bold;
        t.enableWordWrapping = false;
        return btn;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────
    private void OnAccept()
    {
        PlayerPrefs.SetInt(KEY_ACCEPTED, CURRENT_VERSION);
        PlayerPrefs.Save();
        Debug.Log("[TermsUI] Terms accepted by player.");
        Destroy(_panel);
    }

    private void OnDecline()
    {
        Debug.Log("[TermsUI] Terms declined — quitting application.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Terms text (abbreviated summary for in-app display) ───────────────────
    private static string GetTermsText()
    {
        return
@"<b>TERMS AND CONDITIONS — BIG MONEY SLOTS</b>
Effective Date: April 3, 2024
Copyright © 2024 CeeMoreBooty / Big Money Slots. All Rights Reserved.

<b>1. ELIGIBILITY</b>
You must be 18+ years of age to use this App. Big Money Slots is a FREE-TO-PLAY social casino game. It does not involve real money gambling. All virtual currency has no real-world monetary value.

<b>2. IN-APP PURCHASES</b>
You may purchase virtual currency packages with real money. All virtual items are for entertainment only and carry no real-world value.

<b>3. NO REFUND POLICY</b>
<color=#FF4444><b>ALL PURCHASES ARE FINAL AND NON-REFUNDABLE.</b></color>
By purchasing, you agree:
• No refunds for accidental purchases, unused items, account termination, or dissatisfaction.
• Initiating a chargeback may result in permanent account termination.
• Platform refunds are at the platform's discretion and may suspend your account.

<b>4. USER CONDUCT</b>
Do not cheat, hack, use bots, exploit bugs, harass other users, or violate any applicable law.

<b>5. PRIVACY</b>
We collect device information, IP address, and usage analytics for security and service improvement. We do not sell your data.

<b>6. INTELLECTUAL PROPERTY</b>
All content is the exclusive property of CeeMoreBooty / Big Money Slots. Unauthorized use, copying, or distribution is strictly prohibited.

<b>7. ACCOUNT TERMINATION</b>
We may suspend or permanently terminate accounts for violations. No refunds upon termination.

<b>8. DISCLAIMER</b>
This app is provided 'as is'. We are not liable for any damages arising from use of the app.

By tapping ACCEPT & PLAY, you confirm you have read, understood, and agree to these Terms and Conditions, including the <b>No Refund Policy</b>.

Full Terms: See TERMS_AND_CONDITIONS.md
Contact: tech.crew151@gmail.com";
    }
}
