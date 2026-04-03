using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Voice chat via Unity's Microphone API.
/// For production, wire this to Vivox SDK (Unity Gaming Services → Voice & Text Chat)
/// or an alternative WebRTC plugin.  The stubs below mark every integration point.
/// </summary>
public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance { get; private set; }

    [Header("Audio")]
    public int  sampleRate    = 16000;
    public int  clipSeconds   = 5;      // rolling buffer length
    public int  sendIntervalMs = 200;   // how often compressed audio is sent

    public bool IsMuted        { get; private set; } = false;
    public bool IsInVoiceRoom  { get; private set; } = false;
    public string CurrentRoomId { get; private set; }

    private AudioClip  _micClip;
    private string     _micDevice;
    private bool       _isRecording;

    public static event Action<string> OnSpeakerStarted;   // playerId
    public static event Action<string> OnSpeakerStopped;   // playerId
    public static event Action<bool>   OnMuteChanged;
    public static event Action<string> OnJoinedRoom;
    public static event Action         OnLeftRoom;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Room management ───────────────────────────────────────────────────────
    public void JoinVoiceRoom(string roomId)
    {
        if (IsInVoiceRoom) LeaveVoiceRoom();

        CurrentRoomId = roomId;
        IsInVoiceRoom = true;

        // TODO (Vivox): client.JoinChannelAsync(roomId, ChannelType.NonPositional, …)
        Debug.Log($"[VoiceChat] Joining voice room: {roomId}");

        StartMicrophone();
        StartCoroutine(SignalJoinToBackend(roomId));
        OnJoinedRoom?.Invoke(roomId);
    }

    public void LeaveVoiceRoom()
    {
        if (!IsInVoiceRoom) return;
        StopMicrophone();
        StartCoroutine(SignalLeaveToBackend(CurrentRoomId));
        // TODO (Vivox): client.LeaveChannelAsync(…)
        CurrentRoomId = null;
        IsInVoiceRoom = false;
        OnLeftRoom?.Invoke();
        Debug.Log("[VoiceChat] Left voice room.");
    }

    // ── Mute / unmute ─────────────────────────────────────────────────────────
    public void SetMute(bool muted)
    {
        IsMuted = muted;
        // TODO (Vivox): client.SetTransmissionMode(muted ? TransmissionMode.None : TransmissionMode.All)
        if (muted)  StopMicrophone();
        else        StartMicrophone();
        OnMuteChanged?.Invoke(IsMuted);
        Debug.Log($"[VoiceChat] Mute: {IsMuted}");
    }

    public void ToggleMute() => SetMute(!IsMuted);

    // ── Microphone capture (Unity built-in) ───────────────────────────────────
    private void StartMicrophone()
    {
        if (_isRecording || IsMuted) return;
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceChat] No microphone detected.");
            return;
        }
        _micDevice  = Microphone.devices[0];
        _micClip    = Microphone.Start(_micDevice, true, clipSeconds, sampleRate);
        _isRecording = true;

        // TODO: pipe _micClip audio data through your chosen codec (Opus recommended)
        //       and send compressed frames to Vivox or your WebRTC peer connection.
        Debug.Log($"[VoiceChat] Microphone started: {_micDevice}");
    }

    private void StopMicrophone()
    {
        if (!_isRecording) return;
        Microphone.End(_micDevice);
        _isRecording = false;
        Debug.Log("[VoiceChat] Microphone stopped.");
    }

    private void OnDestroy()
    {
        if (IsInVoiceRoom) LeaveVoiceRoom();
        StopMicrophone();
    }

    // ── Backend signalling ─────────────────────────────────────────────────────
    private IEnumerator SignalJoinToBackend(string roomId)
    {
        string url  = $"{BackendClient.BaseUrl}/api/chat/voice/join";
        string body = $"{{\"roomId\":\"{roomId}\"}}";
        using (var req = new UnityWebRequest(url, "POST"))
        {
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();
        }
    }

    private IEnumerator SignalLeaveToBackend(string roomId)
    {
        string url  = $"{BackendClient.BaseUrl}/api/chat/voice/leave";
        string body = $"{{\"roomId\":\"{roomId}\"}}";
        using (var req = new UnityWebRequest(url, "POST"))
        {
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        yield return req.SendWebRequest();
        }
    }
}
