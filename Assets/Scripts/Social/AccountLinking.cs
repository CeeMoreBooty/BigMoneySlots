using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Account Linking — lets players bind Facebook, Google, phone, and Discord
/// to their Big Money Slots account in exchange for large one-time coin bonuses.
///
/// Bonus schedule:
///   Facebook   → 2.5 Trillion coins (one-time)
///   Google     → 2.5 Trillion coins (one-time)
///   Phone      → 2.5 Trillion coins (one-time)
///   Discord    → 5   Trillion coins (one-time)
/// </summary>
public class AccountLinking : MonoBehaviour
{
    public static AccountLinking Instance { get; private set; }

    public enum LinkProvider { Facebook, Google, Phone, Discord }

    [Serializable]
    public class LinkStatus
    {
        public LinkProvider provider;
        public bool         isLinked;
        public bool         bonusClaimed;
        public string       displayName;   // e.g. "john@gmail.com" or "@JohnDoe#1234"
    }

    // Bonus amounts
    private const long BonusFacebookCoins = 2_500_000_000_000L;  // 2.5T
    private const long BonusGoogleCoins   = 2_500_000_000_000L;  // 2.5T
    private const long BonusPhoneCoins    = 2_500_000_000_000L;  // 2.5T
    private const long BonusDiscordCoins  = 5_000_000_000_000L;  // 5T

    public LinkStatus[] statuses = new LinkStatus[]
    {
        new LinkStatus { provider = LinkProvider.Facebook },
        new LinkStatus { provider = LinkProvider.Google   },
        new LinkStatus { provider = LinkProvider.Phone    },
        new LinkStatus { provider = LinkProvider.Discord  },
    };

    public static event Action<LinkProvider, long> OnLinkedAndBonusGranted;
    public static event Action<LinkProvider>       OnLinkFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadStatuses();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by UI when player taps a Link button.</summary>
    public void BeginLink(LinkProvider provider, string oauthTokenOrValue)
    {
        if (string.IsNullOrEmpty(oauthTokenOrValue))
        {
            OnLinkFailed?.Invoke(provider);
            return;
        }
        StartCoroutine(SendLinkToBackend(provider, oauthTokenOrValue));
    }

    public LinkStatus GetStatus(LinkProvider provider) => statuses[(int)provider];
    public bool IsLinked(LinkProvider provider)        => statuses[(int)provider].isLinked;
    public bool BonusClaimed(LinkProvider provider)    => statuses[(int)provider].bonusClaimed;

    // ── Backend call ─────────────────────────────────────────────────────────

    private IEnumerator SendLinkToBackend(LinkProvider provider, string token)
    {
        string url  = $"{BackendClient.BaseUrl}/api/account/link";
        string body = $"{{\"provider\":\"{provider.ToString().ToLower()}\",\"token\":\"{token}\"}}";

        using var req = new UnityWebRequest(url, "POST");
        req.timeout         = 10;
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type",  "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {PlayerPrefs.GetString("auth_token", "")}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type",  "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {PlayerPrefs.GetString("auth_token", "")}");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<LinkResponse>(req.downloadHandler.text);
                var s    = statuses[(int)provider];
                s.isLinked    = true;
                s.displayName = resp.displayName;

                if (!s.bonusClaimed && resp.bonusGranted)
                {
                    s.bonusClaimed = true;
                    long bonus = BonusForProvider(provider);
                    PlayerEconomy.Instance?.AddCoins(bonus);
                    OnLinkedAndBonusGranted?.Invoke(provider, bonus);
                    Debug.Log($"[AccountLinking] {provider} linked! Bonus: +{bonus:N0} coins");
                }

                SaveStatuses();
            }
            else
            {
                Debug.LogWarning($"[AccountLinking] Link failed for {provider}: {req.error}");
                OnLinkFailed?.Invoke(provider);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public static long BonusForProvider(LinkProvider provider) => provider switch
    {
        LinkProvider.Facebook => BonusFacebookCoins,
        LinkProvider.Google   => BonusGoogleCoins,
        LinkProvider.Phone    => BonusPhoneCoins,
        LinkProvider.Discord  => BonusDiscordCoins,
        _                     => 0L
    };

    public static string BonusLabel(LinkProvider provider) => provider switch
    {
        LinkProvider.Discord => "5T Coins",
        _                    => "2.5T Coins"
    };

    // ── Persistence ───────────────────────────────────────────────────────────

    private void SaveStatuses()
    {
        foreach (var s in statuses)
        {
            PlayerPrefs.SetInt($"link_{s.provider}_linked",  s.isLinked    ? 1 : 0);
            PlayerPrefs.SetInt($"link_{s.provider}_claimed", s.bonusClaimed ? 1 : 0);
            PlayerPrefs.SetString($"link_{s.provider}_name", s.displayName ?? "");
        }
        PlayerPrefs.Save();
    }

    private void LoadStatuses()
    {
        foreach (var s in statuses)
        {
            s.isLinked    = PlayerPrefs.GetInt($"link_{s.provider}_linked",  0) == 1;
            s.bonusClaimed = PlayerPrefs.GetInt($"link_{s.provider}_claimed", 0) == 1;
            s.displayName = PlayerPrefs.GetString($"link_{s.provider}_name", "");
        }
    }

    [Serializable] private class LinkResponse
    {
        public bool   bonusGranted;
        public string displayName;
    }
}
