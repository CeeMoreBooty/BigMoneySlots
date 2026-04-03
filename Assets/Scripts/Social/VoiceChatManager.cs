using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Voice chat via Unity's Microphone API with HTTP-based audio relay.
///
/// How it works (no third-party SDK required):
///   1. Microphone audio is captured as a rolling AudioClip at <see cref="sampleRate"/> Hz.
///   2. Every <see cref="sendIntervalMs"/> ms, the new PCM samples are extracted,
///      converted to 16-bit ints, base64-encoded, and POSTed to /api/chat/voice/audio.
///   3. A parallel coroutine polls GET /api/chat/voice/audio/:roomId every
///      <see cref="sendIntervalMs"/> ms and plays back chunks from other speakers
///      using per-speaker AudioSources on dynamically created child GameObjects.
///
/// For production at scale, replace the HTTP relay with Vivox (Unity Gaming Services)
/// or a WebRTC plugin — the public API (JoinVoiceRoom, LeaveVoiceRoom, SetMute)
/// stays the same regardless of transport.
/// </summary>
public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance { get; private set; }

    [Header("Audio Capture")]
    public int sampleRate     = 16000;  // Hz — 16 kHz is sufficient for voice
    public int clipSeconds    = 10;     // rolling microphone buffer length

    [Header("Relay Settings")]
    [Tooltip("Milliseconds between audio chunk sends/polls.")]
    public int sendIntervalMs = 200;    // 200 ms = 5 chunks/sec per speaker

    // ── State ─────────────────────────────────────────────────────────────────
    public bool   IsMuted       { get; private set; }
    public bool   IsInVoiceRoom { get; private set; }
    public string CurrentRoomId { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public static event Action<string> OnSpeakerStarted;   // speakerId
    public static event Action<string> OnSpeakerStopped;   // speakerId
    public static event Action<bool>   OnMuteChanged;
    public static event Action<string> OnJoinedRoom;
    public static event Action         OnLeftRoom;

    // ── Private ───────────────────────────────────────────────────────────────
    private AudioClip _micClip;
    private string    _micDevice;
    private bool      _isRecording;
    private int       _lastSamplePos;

    // Per-speaker AudioSources keyed by speakerId (device-id hash or player name)
    private readonly Dictionary<string, AudioSource> _speakerSources =
        new Dictionary<string, AudioSource>();

    private Coroutine _sendCoroutine;
    private Coroutine _pollCoroutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() => StopMicrophone();

    // ── Room management ───────────────────────────────────────────────────────

    public void JoinVoiceRoom(string roomId)
    {
        if (IsInVoiceRoom) LeaveVoiceRoom();

        CurrentRoomId = roomId;
        IsInVoiceRoom = true;

        StartMicrophone();
        StartCoroutine(SignalJoinToBackend(roomId));

        _sendCoroutine = StartCoroutine(SendAudioChunks());
        _pollCoroutine = StartCoroutine(PollAudioFromRoom());

        OnJoinedRoom?.Invoke(roomId);
        Debug.Log("[VoiceChat] Joined voice room: " + roomId);
    }

    public void LeaveVoiceRoom()
    {
        if (!IsInVoiceRoom) return;

        if (_sendCoroutine != null) { StopCoroutine(_sendCoroutine); _sendCoroutine = null; }
        if (_pollCoroutine != null) { StopCoroutine(_pollCoroutine); _pollCoroutine = null; }

        StopMicrophone();
        StartCoroutine(SignalLeaveToBackend(CurrentRoomId));

        // Notify back-end any remaining chunk keepers for our speaker ID
        foreach (var kv in _speakerSources)
            Destroy(kv.Value.gameObject);
        _speakerSources.Clear();

        string left = CurrentRoomId;
        CurrentRoomId = null;
        IsInVoiceRoom = false;
        OnLeftRoom?.Invoke();
        Debug.Log("[VoiceChat] Left voice room: " + left);
    }

    // ── Mute / unmute ─────────────────────────────────────────────────────────

    public void SetMute(bool muted)
    {
        IsMuted = muted;
        if (muted) StopMicrophone();
        else       StartMicrophone();
        OnMuteChanged?.Invoke(IsMuted);
        Debug.Log("[VoiceChat] Mute: " + IsMuted);
    }

    public void ToggleMute() => SetMute(!IsMuted);

    // ── Microphone capture ────────────────────────────────────────────────────

    private void StartMicrophone()
    {
        if (_isRecording || IsMuted) return;
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceChat] No microphone detected.");
            return;
        }
        _micDevice   = Microphone.devices[0];
        _micClip     = Microphone.Start(_micDevice, true, clipSeconds, sampleRate);
        _isRecording = true;
        _lastSamplePos = 0;
        Debug.Log("[VoiceChat] Microphone started: " + _micDevice);
    }

    private void StopMicrophone()
    {
        if (!_isRecording) return;
        Microphone.End(_micDevice);
        _isRecording = false;
        Debug.Log("[VoiceChat] Microphone stopped.");
    }

    // ── Audio send coroutine ──────────────────────────────────────────────────

    private IEnumerator SendAudioChunks()
    {
        float interval = sendIntervalMs / 1000f;
        while (IsInVoiceRoom)
        {
            yield return new WaitForSeconds(interval);

            if (!_isRecording || IsMuted || _micClip == null) continue;

            int currentPos = Microphone.GetPosition(_micDevice);
            if (currentPos == _lastSamplePos) continue;

            int sampleCount;
            if (currentPos > _lastSamplePos)
                sampleCount = currentPos - _lastSamplePos;
            else
                sampleCount = (_micClip.samples - _lastSamplePos) + currentPos;

            if (sampleCount <= 0) continue;

            var samples = new float[sampleCount];
            _micClip.GetData(samples, _lastSamplePos);
            _lastSamplePos = currentPos;

            // Encode float PCM → 16-bit little-endian bytes → base64
            var pcm = new byte[sampleCount * 2];
            for (int i = 0; i < sampleCount; i++)
            {
                short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                pcm[i * 2]     = (byte)(s & 0xFF);
                pcm[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            }

            string audioB64 = Convert.ToBase64String(pcm);
            yield return PostAudioChunk(audioB64, sampleCount);
        }
    }

    private IEnumerator PostAudioChunk(string audioB64, int sampleCount)
    {
        string url  = BackendClient.BaseUrl + "/api/chat/voice/audio";
        string body = "{\"roomId\":\"" + CurrentRoomId + "\"" +
                      ",\"audioData\":\"" + audioB64 + "\"" +
                      ",\"sampleRate\":" + sampleRate +
                      ",\"sampleCount\":" + sampleCount + "}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        req.timeout = 3;
        yield return req.SendWebRequest();
        // Fire-and-forget — audio relay errors must never block gameplay.
    }

    // ── Audio poll and playback coroutine ─────────────────────────────────────

    private IEnumerator PollAudioFromRoom()
    {
        float interval = sendIntervalMs / 1000f;
        string myId    = DeviceTracker.Instance != null
                         ? DeviceTracker.Instance.DeviceId
                         : SystemInfo.deviceUniqueIdentifier;

        while (IsInVoiceRoom)
        {
            yield return new WaitForSeconds(interval);
            if (CurrentRoomId == null) yield break;

            string url = BackendClient.BaseUrl + "/api/chat/voice/audio/" +
                         CurrentRoomId + "?exclude=" + UnityWebRequest.EscapeURL(myId);

            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
            req.timeout = 3;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) continue;

            ParseAndPlayChunks(req.downloadHandler.text);
        }
    }

    /// <summary>
    /// Parses the JSON array of audio chunks and plays each one through the
    /// matching per-speaker AudioSource.  JSON is parsed manually to avoid
    /// pulling in System.Text.Json or a third-party library.
    /// </summary>
    private void ParseAndPlayChunks(string json)
    {
        // Expected: {"chunks":[{"speakerId":"…","audioData":"…","sampleRate":16000,"sampleCount":3200},…]}
        int chunksIdx = json.IndexOf("\"chunks\"", StringComparison.Ordinal);
        if (chunksIdx < 0) return;

        // Walk through each chunk object
        int pos = json.IndexOf('[', chunksIdx);
        while (pos < json.Length)
        {
            int objStart = json.IndexOf('{', pos);
            if (objStart < 0) break;
            int objEnd = json.IndexOf('}', objStart);
            if (objEnd < 0) break;
            pos = objEnd + 1;

            string obj = json.Substring(objStart, objEnd - objStart + 1);

            string speakerId  = ExtractString(obj, "speakerId");
            string audioB64   = ExtractString(obj, "audioData");
            int    sr         = ExtractInt(obj,    "sampleRate",  sampleRate);
            int    sc         = ExtractInt(obj,    "sampleCount", 0);

            if (string.IsNullOrEmpty(speakerId) ||
                string.IsNullOrEmpty(audioB64)  ||
                sc <= 0) continue;

            try
            {
                byte[]  bytes   = Convert.FromBase64String(audioB64);
                float[] samples = new float[sc];
                int     take    = Mathf.Min(bytes.Length / 2, sc);
                for (int i = 0; i < take; i++)
                {
                    short s = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
                    samples[i] = s / 32767f;
                }

                PlaySpeakerChunk(speakerId, samples, sc, sr);
                OnSpeakerStarted?.Invoke(speakerId);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[VoiceChat] Failed to decode chunk from " + speakerId +
                                 ": " + ex.Message);
            }
        }
    }

    private void PlaySpeakerChunk(string speakerId, float[] samples, int sampleCount, int rate)
    {
        if (!_speakerSources.TryGetValue(speakerId, out AudioSource src) || src == null)
        {
            var go = new GameObject("Voice_" + speakerId);
            go.transform.SetParent(transform);
            src = go.AddComponent<AudioSource>();
            src.spatialBlend = 0f;  // 2D — no spatial audio for now
            src.volume       = 1f;
            _speakerSources[speakerId] = src;
        }

        var clip = AudioClip.Create("chunk_" + speakerId, sampleCount, 1, rate, false);
        clip.SetData(samples, 0);
        src.PlayOneShot(clip);
    }

    // ── Backend signalling ────────────────────────────────────────────────────

    private IEnumerator SignalJoinToBackend(string roomId)
    {
        string url  = BackendClient.BaseUrl + "/api/chat/voice/join";
        string body = "{\"roomId\":\"" + roomId + "\"}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        yield return req.SendWebRequest();
    }

    private IEnumerator SignalLeaveToBackend(string roomId)
    {
        if (roomId == null) yield break;
        string url  = BackendClient.BaseUrl + "/api/chat/voice/leave";
        string body = "{\"roomId\":\"" + roomId + "\"}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        yield return req.SendWebRequest();
    }

    // ── JSON parsing helpers ──────────────────────────────────────────────────

    private static string ExtractString(string json, string key)
    {
        string search = "\"" + key + "\":\"";
        int idx = json.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return null;
        int start = idx + search.Length;
        int end   = json.IndexOf('"', start);
        return end > start ? json.Substring(start, end - start) : null;
    }

    private static int ExtractInt(string json, string key, int fallback)
    {
        string search = "\"" + key + "\":";
        int idx = json.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return fallback;
        int start = idx + search.Length;
        int end   = start;
        while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
        return int.TryParse(json.Substring(start, end - start), out int v) ? v : fallback;
    }
}
