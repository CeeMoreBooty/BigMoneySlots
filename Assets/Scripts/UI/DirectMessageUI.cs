using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SMS-style Direct Message UI.
///
/// Two views that sit inside the same panel:
///   Inbox view  — one row per friend showing their name + latest message preview.
///   Thread view — full scrollable conversation with one friend plus a send bar.
///
/// Wires directly to ChatManager (Friends channel) for send/receive.
/// Wires to FriendsManager to populate the inbox friend list.
///
/// Inspector assignments
/// ─────────────────────
/// panel              — root GameObject toggled by Show/Hide
/// inboxView          — parent of inboxContainer + inbox header
/// threadView         — parent of threadContainer + thread header + send bar
///
/// inboxContainer     — ScrollRect content transform (rows spawned here)
/// inboxRowPrefab     — prefab with children: "NameText" (TMP_Text), "PreviewText" (TMP_Text)
///
/// threadHeader       — TMP_Text showing the open friend's name
/// threadContainer    — ScrollRect content transform (message bubbles spawned here)
/// threadScrollRect   — ScrollRect for auto-scroll to bottom
/// messageBubblePrefab— prefab with a single TMP_Text child (the message text)
///
/// inputField         — TMP_InputField for composing a message
/// sendButton         — Button that fires OnSendClicked
/// backButton         — Button that returns to inbox view
/// </summary>
public class DirectMessageUI : MonoBehaviour
{
    public static DirectMessageUI Instance { get; private set; }
    [Header("Panel")]
    public GameObject panel;

    [Header("Views")]
    public GameObject inboxView;
    public GameObject threadView;

    [Header("Inbox")]
    public Transform  inboxContainer;
    public GameObject inboxRowPrefab;

    [Header("Thread")]
    public TMP_Text   threadHeader;
    public Transform  threadContainer;
    public ScrollRect threadScrollRect;
    public GameObject messageBubblePrefab;

    [Header("Send Bar")]
    public TMP_InputField inputField;
    public Button         sendButton;

    [Header("Navigation")]
    public Button backButton;

    // ── State ─────────────────────────────────────────────────────────────────
    private string _openFriendId;
    private string _openFriendName;

    // Cache: friendId → last-received message preview
    private readonly Dictionary<string, string> _previews = new Dictionary<string, string>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        sendButton?.onClick.AddListener(OnSendClicked);
        inputField?.onSubmit.AddListener(_ => OnSendClicked());
        backButton?.onClick.AddListener(ShowInbox);
    }

    private void OnEnable()
    {
        ChatManager.OnMessageReceived   += OnMessageReceived;
        FriendsManager.OnFriendsUpdated += RebuildInbox;
        RebuildInbox();
    }

    private void OnDisable()
    {
        ChatManager.OnMessageReceived   -= OnMessageReceived;
        FriendsManager.OnFriendsUpdated -= RebuildInbox;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        panel?.SetActive(true);
        ShowInbox();
    }

    public void Hide() => panel?.SetActive(false);

    /// <summary>Open the DM thread with a specific friend directly.</summary>
    public void OpenThread(string friendId, string friendName)
    {
        panel?.SetActive(true);
        OpenThreadInternal(friendId, friendName);
    }

    // ── Inbox ─────────────────────────────────────────────────────────────────

    private void ShowInbox()
    {
        _openFriendId = null;
        inboxView?.SetActive(true);
        threadView?.SetActive(false);
        RebuildInbox();
    }

    private void RebuildInbox()
    {
        if (inboxContainer == null || inboxRowPrefab == null) return;

        foreach (Transform t in inboxContainer) Destroy(t.gameObject);

        var friends = FriendsManager.Instance?.Friends ?? new List<FriendsManager.Friend>();
        if (friends.Count == 0)
        {
            var empty = Instantiate(inboxRowPrefab, inboxContainer);
            SetInboxRowText(empty, "No friends yet", "Add friends to start messaging");
            return;
        }

        foreach (var f in friends)
        {
            var row = Instantiate(inboxRowPrefab, inboxContainer);
            _previews.TryGetValue(f.playerId, out string preview);
            SetInboxRowText(row, f.displayName, preview ?? "Tap to start chatting…");

            string id   = f.playerId;
            string name = f.displayName;
            var btn = row.GetComponent<Button>() ?? row.AddComponent<Button>();
            btn.onClick.AddListener(() => OpenThreadInternal(id, name));
        }
    }

    private static void SetInboxRowText(GameObject row, string nameStr, string previewStr)
    {
        var nameText    = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var previewText = row.transform.Find("PreviewText")?.GetComponent<TMP_Text>();
        if (nameText    != null) nameText.text    = nameStr;
        if (previewText != null) previewText.text = previewStr;
    }

    // ── Thread ────────────────────────────────────────────────────────────────

    private void OpenThreadInternal(string friendId, string friendName)
    {
        _openFriendId   = friendId;
        _openFriendName = friendName;

        ChatManager.Instance?.SetDMTarget(friendId);
        ChatManager.Instance?.LoadDMThread(friendId);   // fetch history from backend

        if (threadHeader != null) threadHeader.text = friendName;

        inboxView?.SetActive(false);
        threadView?.SetActive(true);

        RebuildThread();
    }

    private void RebuildThread()
    {
        if (threadContainer == null || messageBubblePrefab == null) return;
        foreach (Transform t in threadContainer) Destroy(t.gameObject);

        var history = ChatManager.Instance?.GetHistory(ChatManager.ChatChannel.Friends)
                      ?? new List<ChatManager.ChatMessage>();

        string myId = PlayerPrefs.GetString("player_id", "local");
        foreach (var msg in history)
        {
            bool isDM = msg.senderId == _openFriendId || msg.targetId == _openFriendId
                     || msg.senderId == myId;
            if (!isDM) continue;
            AppendBubble(msg);
        }

        ScrollThreadToBottom();
    }

    private void AppendBubble(ChatManager.ChatMessage msg)
    {
        if (messageBubblePrefab == null || threadContainer == null) return;

        var bubble   = Instantiate(messageBubblePrefab, threadContainer);
        var tmpText  = bubble.GetComponentInChildren<TMP_Text>();
        if (tmpText == null) return;

        string myId   = PlayerPrefs.GetString("player_id", "local");
        bool   isMine = msg.senderId == myId;

        string timestamp = DateTimeOffset.FromUnixTimeMilliseconds(msg.timestamp)
                                         .ToLocalTime()
                                         .ToString("h:mm tt");

        // Bold sender name, then message, then small timestamp
        tmpText.text = isMine
            ? $"<b>You</b>  <size=70%>{timestamp}</size>\n{msg.text}"
            : $"<b>{msg.senderName}</b>  <size=70%>{timestamp}</size>\n{msg.text}";

        // Right-align own messages, left-align received
        tmpText.alignment = isMine
            ? TextAlignmentOptions.Right
            : TextAlignmentOptions.Left;
    }

    private void ScrollThreadToBottom()
    {
        if (threadScrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        threadScrollRect.verticalNormalizedPosition = 0f;
    }

    // ── Send ─────────────────────────────────────────────────────────────────

    private void OnSendClicked()
    {
        if (string.IsNullOrEmpty(_openFriendId)) return;
        string text = inputField?.text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        ChatManager.Instance?.SendMessage(
            ChatManager.ChatChannel.Friends,
            text,
            targetId: _openFriendId
        );

        inputField.text = "";
    }

    // ── Receive ───────────────────────────────────────────────────────────────

    private void OnMessageReceived(ChatManager.ChatChannel channel, ChatManager.ChatMessage msg)
    {
        if (channel != ChatManager.ChatChannel.Friends) return;

        // null msg = full-refresh signal from LoadDMThread
        if (msg == null)
        {
            if (_openFriendId != null) RebuildThread();
            return;
        }

        // Update inbox preview for this friend
        string friendId = msg.senderId == PlayerPrefs.GetString("player_id", "local")
            ? msg.targetId
            : msg.senderId;
        if (!string.IsNullOrEmpty(friendId))
            _previews[friendId] = msg.text.Length > 40 ? msg.text.Substring(0, 40) + "…" : msg.text;

        // If thread is open for this friend, append the bubble live
        if (_openFriendId == friendId || _openFriendId == msg.senderId)
        {
            AppendBubble(msg);
            ScrollThreadToBottom();
        }
    }
}
