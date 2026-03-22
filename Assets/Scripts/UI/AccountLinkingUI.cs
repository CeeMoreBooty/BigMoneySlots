using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Account Linking UI Panel.
/// Shows link status for Facebook, Google, Phone, and Discord.
/// Displays bonus amount and Claim button for each unlinked provider.
/// </summary>
public class AccountLinkingUI : MonoBehaviour
{
    [System.Serializable]
    public class ProviderRow
    {
        public AccountLinking.LinkProvider provider;
        public GameObject row;
        public TMP_Text   statusText;
        public TMP_Text   bonusText;
        public Button     linkButton;
        public GameObject linkedBadge;
    }

    [Header("Panel")]
    public GameObject panel;

    [Header("Provider Rows")]
    public ProviderRow[] rows;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    private void Awake()
    {
        foreach (var r in rows)
        {
            var captured = r;
            r.linkButton?.onClick.AddListener(() => OnLinkClicked(captured.provider));
        }
    }

    private void OnEnable()
    {
        AccountLinking.OnLinkedAndBonusGranted += HandleLinked;
        AccountLinking.OnLinkFailed            += HandleFailed;
        Refresh();
    }

    private void OnDisable()
    {
        AccountLinking.OnLinkedAndBonusGranted -= HandleLinked;
        AccountLinking.OnLinkFailed            -= HandleFailed;
    }

    public void Show() { panel?.SetActive(true); Refresh(); }
    public void Hide() => panel?.SetActive(false);

    private void Refresh()
    {
        if (AccountLinking.Instance == null) return;
        foreach (var r in rows)
        {
            var status = AccountLinking.Instance.GetStatus(r.provider);
            bool linked = status.isLinked;

            r.linkedBadge?.SetActive(linked);
            r.linkButton?.gameObject.SetActive(!linked);

            if (r.statusText != null)
                r.statusText.text = linked
                    ? $"✅ {r.provider}  —  {status.displayName}"
                    : $"🔗 Link {r.provider}";

            if (r.bonusText != null)
                r.bonusText.text = linked
                    ? "Bonus claimed!"
                    : $"+{AccountLinking.BonusLabel(r.provider)}";
        }
    }

    private void OnLinkClicked(AccountLinking.LinkProvider provider)
    {
        // In production, open the OAuth flow for Facebook / Google / Discord
        // or show a phone-number input field.
        // For editor / prototype, simulate with a dummy token.
        SetFeedback($"Opening {provider} login…");

#if UNITY_EDITOR
        AccountLinking.Instance?.BeginLink(provider, $"editor_test_token_{provider}");
#else
        // TODO: Trigger native OAuth plugin per provider
        // Facebook:  FB.LogInWithReadPermissions(…)
        // Google:    GoogleSignIn.DefaultInstance.SignIn()
        // Discord:   Open OAuth URL via Application.OpenURL(discordOAuthUrl)
        // Phone:     Show phone input dialog → send to backend for SMS OTP
        SetFeedback($"{provider} OAuth not yet wired on device. See AccountLinkingUI.cs TODO.");
#endif
    }

    private void HandleLinked(AccountLinking.LinkProvider provider, long bonus)
    {
        SetFeedback($"🎉 {provider} linked! +{FormatCoins(bonus)} coins added!");
        Refresh();
    }

    private void HandleFailed(AccountLinking.LinkProvider provider)
    {
        SetFeedback($"❌ Failed to link {provider}. Please try again.");
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
        Debug.Log($"[AccountLinkingUI] {msg}");
    }

    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000_000L) return $"{v / 1_000_000_000_000L}T";
        if (v >= 1_000_000_000L)     return $"{v / 1_000_000_000L}B";
        return v.ToString("N0");
    }
}
