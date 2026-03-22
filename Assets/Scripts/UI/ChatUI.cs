using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Chat UI — scrollable message history with tabs for Global, Room, and Friends DM.
/// </summary>
public class ChatUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Channel Tabs")]
    public Button tabGlobal;
    public Button tabRoom;
    public Button tabFriends;

    [Header("Messages")]
    public Transform      messageContainer;
    public GameObject     messageRowPrefab;   // TMP_Text child named "Text"
    public ScrollRect     scrollRect;

    [Header("Input")]
    public TMP_InputField inputField;
    public Button         sendButton;

    [Header("Voice")]
    public Button   voiceJoinButton;
    public Button   voiceMuteButton;
    public TMP_Text voiceStatusText;

    private ChatManager.ChatChannel _activeChannel = ChatManager.ChatChannel.Global;

    private void Awake()
    {
        tabGlobal?.onClick.AddListener(()  => SwitchChannel(ChatManager.ChatChannel.Global));
        tabRoom?.onClick.AddListener(()    => SwitchChannel(ChatManager.ChatChannel.Room));
        tabFriends?.onClick.AddListener(() => SwitchChannel(ChatManager.ChatChannel.Friends));
        sendButton?.onClick.AddListener(OnSendClicked);
        inputField?.onSubmit.AddListener(_ => OnSendClicked());
        voiceJoinButton?.onClick.AddListener(OnVoiceJoinClicked);
        voiceMuteButton?.onClick.AddListener(() => VoiceChatManager.Instance?.ToggleMute());
    }

    private void OnEnable()
    {
        ChatManager.OnMessageReceived  += OnMessageReceived;
        VoiceChatManager.OnMuteChanged += OnMuteChanged;
        VoiceChatManager.OnJoinedRoom  += r => UpdateVoiceStatus($"🎙️ In room: {r}");
        VoiceChatManager.OnLeftRoom    += () => UpdateVoiceStatus("🔇 Not in voice");
        RebuildHistory();
    }

    private void OnDisable()
    {
        ChatManager.OnMessageReceived  -= OnMessageReceived;
        VoiceChatManager.OnMuteChanged -= OnMuteChanged;
    }

    public void Show() { panel?.SetActive(true);  RebuildHistory(); }
    public void Hide() => panel?.SetActive(false);

    // ── Channel switch ────────────────────────────────────────────────────────
    private void SwitchChannel(ChatManager.ChatChannel ch)
    {
        _activeChannel = ch;
        RebuildHistory();
    }

    // ── Send ─────────────────────────────────────────────────────────────────
    private void OnSendClicked()
    {
        string text = inputField?.text?.Trim();
        if (string.IsNullOrEmpty(text)) return;
        ChatManager.Instance?.SendMessage(_activeChannel, text);
        inputField.text = "";
    }

    // ── Receive ───────────────────────────────────────────────────────────────
    private void OnMessageReceived(ChatManager.ChatChannel ch, ChatManager.ChatMessage msg)
    {
        if (ch != _activeChannel) return;
        AppendRow(msg);
        ScrollToBottom();
    }

    // ── Build history ─────────────────────────────────────────────────────────
    private void RebuildHistory()
    {
        if (messageContainer == null) return;
        foreach (Transform t in messageContainer) Destroy(t.gameObject);

        var history = ChatManager.Instance?.GetHistory(_activeChannel)
                      ?? new List<ChatManager.ChatMessage>();
        foreach (var m in history) AppendRow(m);
        ScrollToBottom();
    }

    private void AppendRow(ChatManager.ChatMessage msg)
    {
        if (messageRowPrefab == null || messageContainer == null) return;
        var row  = Instantiate(messageRowPrefab, messageContainer);
        var text = row.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = $"<b>{msg.senderName}</b>: {msg.text}";
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
            Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    // ── Voice ─────────────────────────────────────────────────────────────────
    private void OnVoiceJoinClicked()
    {
        if (VoiceChatManager.Instance == null) return;
        if (VoiceChatManager.Instance.IsInVoiceRoom)
            VoiceChatManager.Instance.LeaveVoiceRoom();
        else
            VoiceChatManager.Instance.JoinVoiceRoom($"room_{_activeChannel}");
    }

    private void OnMuteChanged(bool muted)
    {
        if (voiceMuteButton != null)
        {
            var t = voiceMuteButton.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = muted ? "🔇 Unmute" : "🎙️ Mute";
        }
    }

    private void UpdateVoiceStatus(string msg)
    {
        if (voiceStatusText != null) voiceStatusText.text = msg;
    }
}
