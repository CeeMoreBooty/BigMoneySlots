using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Friends system — add/remove friends, view online status,
/// accept/decline requests, and send gifts.
/// </summary>
public class FriendsManager : MonoBehaviour
{
    public static FriendsManager Instance { get; private set; }

    [Serializable]
    public class Friend
    {
        public string playerId;
        public string displayName;
        public bool   isOnline;
        public string currentGame;
        public long   lastSeen;
        public bool   isPending;    // true = incoming request not yet accepted
        public bool   isOutgoing;   // true = we sent the request
    }

    public List<Friend> Friends  { get; private set; } = new();
    public List<Friend> Pending  { get; private set; } = new();

    public static event Action           OnFriendsUpdated;
    public static event Action<string>   OnFriendRequestReceived;   // displayName
    public static event Action<string>   OnFriendAccepted;

    private Coroutine _pollCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FetchFriends();
        _pollCoroutine = StartCoroutine(PollFriends());
    }

    // ── Public actions ────────────────────────────────────────────────────────

    public void SendFriendRequest(string targetPlayerId) =>
        StartCoroutine(PostFriendAction("request", targetPlayerId));

    public void AcceptFriendRequest(string targetPlayerId) =>
        StartCoroutine(PostFriendAction("accept",  targetPlayerId));

    public void DeclineFriendRequest(string targetPlayerId) =>
        StartCoroutine(PostFriendAction("decline", targetPlayerId));

    public void RemoveFriend(string targetPlayerId) =>
        StartCoroutine(PostFriendAction("remove",  targetPlayerId));

    public void FetchFriends() =>
        StartCoroutine(GetFriends());

    // ── REST calls ────────────────────────────────────────────────────────────

    private IEnumerator GetFriends()
    {
        string url = $"{BackendClient.BaseUrl}/api/friends";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {PlayerPrefs.GetString("auth_token", "")}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var wrapper = JsonUtility.FromJson<FriendsWrapper>(req.downloadHandler.text);
            if (wrapper != null)
            {
                Friends = wrapper.friends  ?? new List<Friend>();
                Pending = wrapper.pending  ?? new List<Friend>();
                OnFriendsUpdated?.Invoke();
            }
        }
    }

    private IEnumerator PostFriendAction(string action, string targetPlayerId)
    {
        string url  = $"{BackendClient.BaseUrl}/api/friends/{action}";
        string body = $"{{\"targetPlayerId\":\"{targetPlayerId}\"}}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type",  "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {PlayerPrefs.GetString("auth_token", "")}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            if (action == "accept") OnFriendAccepted?.Invoke(targetPlayerId);
            FetchFriends();
        }
    }

    private IEnumerator PollFriends()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            FetchFriends();
        }
    }

    [Serializable] private class FriendsWrapper
    {
        public List<Friend> friends;
        public List<Friend> pending;
    }
}
