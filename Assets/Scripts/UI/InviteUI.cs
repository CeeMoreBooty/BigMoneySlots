using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Invite Reward UI Panel.
///
/// Layout expected in Inspector:
///   panel            — root GameObject to show/hide
///   myCodeText       — displays the player's personal invite code
///   copyButton       — copies code to clipboard
///   totalInvitesText — "X friends invited"
///   gemsEarnedText   — "X gems earned"
///   rewardInfoText   — static label describing the reward amounts
///   redeemInput      — TMP_InputField for entering a friend's code
///   redeemButton     — submits the code
///   feedbackText     — success / error feedback line
/// </summary>
public class InviteUI : MonoBehaviour
{
    public static InviteUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("My Code")]
    public TMP_Text myCodeText;
    public Button   copyButton;

    [Header("Stats")]
    public TMP_Text totalInvitesText;
    public TMP_Text gemsEarnedText;
    public TMP_Text rewardInfoText;

    [Header("Redeem")]
    public TMP_InputField redeemInput;
    public Button         redeemButton;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        copyButton?.onClick.AddListener(OnCopyClicked);
        redeemButton?.onClick.AddListener(OnRedeemClicked);
        redeemInput?.onSubmit.AddListener(_ => OnRedeemClicked());
    }

    private void OnEnable()
    {
        InviteRewardManager.OnCodeFetched        += Refresh;
        InviteRewardManager.OnCodeRedeemed       += HandleRedeemed;
        InviteRewardManager.OnRedeemFailed       += HandleFailed;
        InviteRewardManager.OnInviteRewardReceived += HandleInviteEarned;
        Refresh();
    }

    private void OnDisable()
    {
        InviteRewardManager.OnCodeFetched        -= Refresh;
        InviteRewardManager.OnCodeRedeemed       -= HandleRedeemed;
        InviteRewardManager.OnRedeemFailed       -= HandleFailed;
        InviteRewardManager.OnInviteRewardReceived -= HandleInviteEarned;
    }

    // ── Show / Hide ───────────────────────────────────────────────────────────

    public void Show()
    {
        panel?.SetActive(true);
        InviteRewardManager.Instance?.FetchMyCode();
    }

    public void Hide() => panel?.SetActive(false);

    // ── Refresh display ───────────────────────────────────────────────────────

    private void Refresh()
    {
        var mgr = InviteRewardManager.Instance;
        if (mgr == null) return;

        if (myCodeText      != null) myCodeText.text      = mgr.MyCode;
        if (totalInvitesText != null) totalInvitesText.text = $"{mgr.TotalInvites} friend{(mgr.TotalInvites == 1 ? "" : "s")} invited";
        if (gemsEarnedText  != null) gemsEarnedText.text  = $"{mgr.GemsEarned} gems earned";
        if (rewardInfoText  != null)
            rewardInfoText.text =
                $"💎 You earn {mgr.GemRewardPerInvite} gems per friend who joins\n" +
                $"🎁 Your friend gets {mgr.GemRewardForNew} gems when they enter your code";
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnCopyClicked()
    {
        string code = InviteRewardManager.Instance?.MyCode ?? "";
        if (string.IsNullOrEmpty(code)) return;
        GUIUtility.systemCopyBuffer = code;
        SetFeedback("✅ Code copied to clipboard!");
    }

    private void OnRedeemClicked()
    {
        string code = redeemInput?.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(code))
        {
            SetFeedback("Please enter an invite code.");
            return;
        }
        redeemButton.interactable = false;
        SetFeedback("Redeeming…");
        InviteRewardManager.Instance?.RedeemCode(code);
    }

    // ── Event callbacks ───────────────────────────────────────────────────────

    private void HandleRedeemed(int gemsAwarded)
    {
        redeemButton.interactable = true;
        if (redeemInput != null) redeemInput.text = "";
        SetFeedback($"🎉 +{gemsAwarded} gems added to your balance!");
        Refresh();
    }

    private void HandleFailed(string error)
    {
        redeemButton.interactable = true;
        SetFeedback($"❌ {error}");
    }

    private void HandleInviteEarned(int gems)
    {
        SetFeedback($"💎 A friend joined using your code! +{gems} gems!");
        Refresh();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
        Debug.Log($"[InviteUI] {msg}");
    }
}
