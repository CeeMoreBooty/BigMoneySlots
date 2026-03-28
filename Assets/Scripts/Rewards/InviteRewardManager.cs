using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Invite Reward Manager — lets players share a personal code and earn gems
/// when friends redeem it.
///
/// Gem rewards (must match Backend/routes/invite.js constants):
///   Code owner (inviter) : 100 gems per unique redemption
///   New player (redeemer): 50  gems one-time for entering any valid code
///
/// Usage:
///   InviteRewardManager.Instance.FetchMyCode();   // on panel open
///   InviteRewardManager.Instance.RedeemCode(code); // on submit
/// </summary>
public class InviteRewardManager : MonoBehaviour
{
    public static InviteRewardManager Instance { get; private set; }

    // ── Runtime state (populated by FetchMyCode) ─────────────────────────────
    public string MyCode           { get; private set; } = "";
    public int    TotalInvites     { get; private set; }
    public int    GemsEarned       { get; private set; }
    public int    GemRewardPerInvite { get; private set; } = 100;
    public int    GemRewardForNew    { get; private set; } = 50;

    // ── Events ────────────────────────────────────────────────────────────────
    public static event Action                OnCodeFetched;          // code + stats refreshed
    public static event Action<int>           OnCodeRedeemed;         // gemsAwarded
    public static event Action<string>        OnRedeemFailed;         // error message
    public static event Action<int>           OnInviteRewardReceived; // realtime: gems earned as inviter

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FetchMyCode();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Fetch (or create) the player's personal invite code from the backend.</summary>
    public void FetchMyCode() => StartCoroutine(GetCode());

    /// <summary>Submit another player's invite code to claim the new-player gem bonus.</summary>
    public void RedeemCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            OnRedeemFailed?.Invoke("Please enter an invite code.");
            return;
        }
        StartCoroutine(PostRedeem(code.Trim().ToUpper()));
    }

    // ── REST calls ────────────────────────────────────────────────────────────

    private IEnumerator GetCode()
    {
        string url = $"{BackendClient.BaseUrl}/api/invite/code";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JsonUtility.FromJson<CodeResponse>(req.downloadHandler.text);
            MyCode             = data.code;
            TotalInvites       = data.totalInvites;
            GemsEarned         = data.gemsEarned;
            GemRewardPerInvite = data.gemRewardPerInvite;
            GemRewardForNew    = data.gemRewardForNew;
            OnCodeFetched?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[InviteRewardManager] FetchMyCode failed: {req.error}");
        }
    }

    private IEnumerator PostRedeem(string code)
    {
        string url  = $"{BackendClient.BaseUrl}/api/invite/redeem";
        string body = $"{{\"code\":\"{code}\"}}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type",  "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JsonUtility.FromJson<RedeemResponse>(req.downloadHandler.text);
            GemSystem.Instance?.AddGems(data.gemsAwarded);
            OnCodeRedeemed?.Invoke(data.gemsAwarded);
            Debug.Log($"[InviteRewardManager] Code redeemed! +{data.gemsAwarded} gems.");
        }
        else
        {
            // Parse error message from backend JSON if possible
            string msg = "Invalid or already-used code.";
            try
            {
                var err = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);
                if (!string.IsNullOrEmpty(err.error)) msg = err.error;
            }
            catch { /* ignore parse failure */ }

            Debug.LogWarning($"[InviteRewardManager] Redeem failed: {msg}");
            OnRedeemFailed?.Invoke(msg);
        }
    }

    // ── Called by socket.io when the backend fires 'invite_redeemed' ──────────
    /// <summary>
    /// Wire this to your socket.io client's OnAny/On("invite_redeemed") callback.
    /// The backend emits this to the inviter's player room when someone uses their code.
    /// </summary>
    public void OnSocketInviteRedeemed(string json)
    {
        var data = JsonUtility.FromJson<InviteRedeemedEvent>(json);
        if (data == null) return;
        GemSystem.Instance?.AddGems(data.gemsEarned);
        OnInviteRewardReceived?.Invoke(data.gemsEarned);
        TotalInvites++;
        GemsEarned += data.gemsEarned;
        Debug.Log($"[InviteRewardManager] {data.redeemerName} used your code! +{data.gemsEarned} gems.");
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────
    [Serializable] private class CodeResponse
    {
        public string code;
        public int    totalInvites;
        public int    gemsEarned;
        public int    gemRewardPerInvite;
        public int    gemRewardForNew;
    }

    [Serializable] private class RedeemResponse
    {
        public bool   success;
        public int    gemsAwarded;
        public int    newGemBalance;
    }

    [Serializable] private class ErrorResponse
    {
        public string error;
    }

    [Serializable] private class InviteRedeemedEvent
    {
        public string redeemerName;
        public int    gemsEarned;
    }
}
