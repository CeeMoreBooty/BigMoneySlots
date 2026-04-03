using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages text chat over a WebSocket connection (socket.io-compatible).
/// Supports three channels: Global, Room (current game), and Friends DM.
/// Connects to the backend socket.io server at BackendClient.BaseUrl.
/// </summary>
public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    public enum ChatChannel { Global, Room, Friends }

    [Serializable]
    public class ChatMessage
    {
        public string   senderId;
        public string   senderName;
        public string   text;
        public string   channel;
        public string   roomId;       // only for Room channel
        public string   targetId;     // only for Friends DM
        public long     timestamp;
    }

    public const int MaxHistoryPerChannel = 100;

    private Dictionary<ChatChannel, List<ChatMessage>> _history = new()
    {
        { ChatChannel.Global,  new List<ChatMessage>() },
        { ChatChannel.Room,    new List<ChatMessage>() },
        { ChatChannel.Friends, new List<ChatMessage>() },
    };

    // Active DM target (playerId)
    public string ActiveDMTarget { get; private set; }

    public static event Action<ChatChannel, ChatMessage> OnMessageReceived;
    public static event Action<string>                   OnConnectionError;
    public static event Action                           OnConnected;

    private bool _connected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => Connect();

    // ── Connection ────────────────────────────────────────────────────────────
    public void Connect()
    {
        // NOTE: Unity does not include a socket.io client by default.
        // Recommended package: "socket.io-client-csharp" (NuGet / Unity Package).
        // Wire up the socket events (OnAny, On("chat_message"), etc.) here once
        // the package is imported. The stubs below show the intended integration.
        Debug.Log("[ChatManager] Connecting to chat server…");
        StartCoroutine(LoadRecentHistory(ChatChannel.Global));
        _connected = true;
        OnConnected?.Invoke();
    }

    // ── Send message ─────────────────────────────────────────────────────────
    public void SendMessage(ChatChannel channel, string text, string roomId = null, string targetId = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        text = SanitizeText(text);

        string rawName   = PlayerPrefs.GetString("player_name", "Guardian");
        string clampedName = rawName.Length > 24 ? rawName.Substring(0, 24) : rawName;

        var msg = new ChatMessage
        {
            senderId   = PlayerPrefs.GetString("player_id", "local"),
            senderName = clampedName,
            text       = text,
            channel    = channel.ToString().ToLower(),
            roomId     = roomId,
            targetId   = targetId ?? ActiveDMTarget,
            timestamp  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        // TODO: emit via socket.io: socket.Emit("chat_message", JsonUtility.ToJson(msg));
        // For now fall back to REST POST
        StartCoroutine(PostMessage(msg));
        AddToHistory(channel, msg);
    }

    public void SetDMTarget(string playerId) => ActiveDMTarget = playerId;

    public List<ChatMessage> GetHistory(ChatChannel channel) =>
        _history.TryGetValue(channel, out var list) ? list : new List<ChatMessage>();

    // ── Called by socket.io callback when a message arrives from server ───────
    public void OnSocketMessageReceived(string json)
    {
        ChatMessage msg;
        try
        {
            msg = JsonUtility.FromJson<ChatMessage>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ChatManager] Failed to parse socket message: {ex.Message}");
            return;
        }
        if (msg == null) return;
        ChatChannel channel = msg.channel switch
        {
            "room"    => ChatChannel.Room,
            "friends" => ChatChannel.Friends,
            _         => ChatChannel.Global,
        };
        AddToHistory(channel, msg);
        OnMessageReceived?.Invoke(channel, msg);
    }

    // ── REST fallback ─────────────────────────────────────────────────────────
    private IEnumerator PostMessage(ChatMessage msg)
    {
        string url  = $"{BackendClient.BaseUrl}/api/chat/send";
        string body = JsonUtility.ToJson(msg);
        using var req = new UnityWebRequest(url, "POST");
        req.timeout         = 10;
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[ChatManager] Send failed: {req.error}");
    }

    private IEnumerator LoadRecentHistory(ChatChannel channel)
    {
        string url = $"{BackendClient.BaseUrl}/api/chat/history?channel={channel.ToString().ToLower()}&limit=50";
        using var req = UnityWebRequest.Get(url);
        req.timeout = 10;
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            // Parse array — simplified; use a wrapper class for production
            Debug.Log($"[ChatManager] History loaded for {channel}");
        }
    }

    /// <summary>
    /// Fetches the full DM conversation with <paramref name="friendId"/> from the backend
    /// and merges it into the Friends channel history. Call this when opening a DM thread.
    /// </summary>
    public void LoadDMThread(string friendId) =>
        StartCoroutine(FetchDMThread(friendId));

    private IEnumerator FetchDMThread(string friendId)
    {
        string url = $"{BackendClient.BaseUrl}/api/chat/dm/thread/{friendId}?limit=100";
        using var req = UnityWebRequest.Get(url);
        req.timeout = 10;
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[ChatManager] LoadDMThread failed: {req.error}");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<ChatMessageListWrapper>(req.downloadHandler.text);
        if (wrapper?.messages == null) yield break;

        // Merge into Friends history (avoid duplicates by timestamp)
        var existing = _history[ChatChannel.Friends];
        var existingTs = new System.Collections.Generic.HashSet<long>();
        foreach (var m in existing) existingTs.Add(m.timestamp);

        foreach (var m in wrapper.messages)
        {
            if (!existingTs.Contains(m.timestamp))
                existing.Add(m);
        }
        // Re-sort chronologically
        existing.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
        // Trim to cap
        while (existing.Count > MaxHistoryPerChannel) existing.RemoveAt(0);

        OnMessageReceived?.Invoke(ChatChannel.Friends, null); // null signals a full refresh
    }

    [Serializable] private class ChatMessageListWrapper { public List<ChatMessage> messages; }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void AddToHistory(ChatChannel channel, ChatMessage msg)
    {
        var list = _history[channel];
        list.Add(msg);
        if (list.Count > MaxHistoryPerChannel) list.RemoveAt(0);
    }

    private static string SanitizeText(string text)
    {
        // Strip HTML tags and trim; profanity filter can be added here
        text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "").Trim();
        return text.Substring(0, Math.Min(text.Length, 200));
    }
}
