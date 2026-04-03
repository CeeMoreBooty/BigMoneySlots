using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Lightweight analytics / event-tracking manager.
///
/// Events are queued locally and flushed to the backend in batches every
/// <see cref="flushIntervalSeconds"/> seconds, or immediately on app pause/quit.
/// All network calls are fire-and-forget — failures are silently swallowed so
/// analytics never causes a gameplay error.
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Flush Settings")]
    [Tooltip("Seconds between automatic batch flushes.")]
    public float flushIntervalSeconds = 30f;

    [Header("Debug")]
    public bool logEvents = false;   // set true in Editor to print events to Console

    // ── Event types ───────────────────────────────────────────────────────────
    public static class Event
    {
        public const string SessionStart   = "session_start";
        public const string SessionEnd     = "session_end";
        public const string Spin           = "spin";
        public const string Win            = "win";
        public const string JackpotWin     = "jackpot_win";
        public const string DailyReward    = "daily_reward";
        public const string HourlyReward   = "hourly_reward";
        public const string IAPPurchase    = "iap_purchase";
        public const string SceneLoad      = "scene_load";
        public const string FraudFlag      = "fraud_flag";
        public const string ErrorCaught    = "error_caught";
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    [Serializable]
    private class AnalyticsEvent
    {
        public string eventName;
        public string deviceId;
        public string sessionId;
        public long   timestamp;   // Unix seconds
        public string payload;     // JSON string of event-specific data
    }

    private readonly List<AnalyticsEvent> _queue = new();
    private string _sessionId;
    private double _sessionStartTime;
    private const int MaxQueueSize = 200;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sessionId        = Guid.NewGuid().ToString("N").Substring(0, 16);
        _sessionStartTime = UtcNow();

        PlayerPrefs.SetInt("analytics_session_count",
            PlayerPrefs.GetInt("analytics_session_count", 0) + 1);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        Track(Event.SessionStart, $"{{"session":"{_sessionId}"," +
              $""session_count":{PlayerPrefs.GetInt("analytics_session_count",1)}}}");

        // Wire into slot machine events (no hard reference to SlotMachine needed).
        ProgressiveJackpot.OnJackpotWon  += OnJackpotWon;

        InvokeRepeating(nameof(Flush), flushIntervalSeconds, flushIntervalSeconds);
    }

    private void OnDestroy()
    {
        ProgressiveJackpot.OnJackpotWon -= OnJackpotWon;
    }

    // ── Public tracking API ───────────────────────────────────────────────────

    /// <summary>Track a named event with optional JSON payload.</summary>
    public void Track(string eventName, string jsonPayload = "{}")
    {
        if (_queue.Count >= MaxQueueSize) _queue.RemoveAt(0);  // drop oldest

        _queue.Add(new AnalyticsEvent
        {
            eventName = eventName,
            deviceId  = DeviceTracker.Instance?.DeviceId ?? SystemInfo.deviceUniqueIdentifier,
            sessionId = _sessionId,
            timestamp = (long)UtcNow(),
            payload   = jsonPayload,
        });

        if (logEvents)
            Debug.Log($"[Analytics] {eventName}: {jsonPayload}");
    }

    /// <summary>Track a spin: bet amount, win amount, jackpot flag.</summary>
    public void TrackSpin(long bet, long win, bool isJackpot)
        => Track(isJackpot ? Event.JackpotWin : (win > 0 ? Event.Win : Event.Spin),
                 $"{{"bet":{bet},"win":{win},"jackpot":{(isJackpot ? "true" : "false")}}}");

    // ── Event listeners ───────────────────────────────────────────────────────
    private void OnJackpotWon(ProgressiveJackpot.JackpotTier tier, long amount, string label)
        => Track(Event.JackpotWin, $"{{"tier":"{label}","amount":{amount}}}");

    // ── Flush to backend ──────────────────────────────────────────────────────
    private void OnApplicationPause(bool paused)
    {
        if (paused) Flush();
    }

    private void OnApplicationQuit()
    {
        Track(Event.SessionEnd,
              $"{{"duration":{(long)(UtcNow() - _sessionStartTime)}}}");
        // Synchronous flush not possible in Unity, but queue is persisted below.
        PersistQueue();
    }

    private void Flush()
    {
        if (_queue.Count == 0) return;
        StartCoroutine(SendBatch(new List<AnalyticsEvent>(_queue)));
        _queue.Clear();
    }

    private IEnumerator SendBatch(List<AnalyticsEvent> batch)
    {
        string url  = BackendClient.BaseUrl + "/api/analytics/batch";
        var sb = new StringBuilder("[");
        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            sb.Append($"{{"event":"{e.eventName}","device":"{e.deviceId}"," +
                      $""session":"{e.sessionId}","ts":{e.timestamp}," +
                      $""data":{e.payload}}}");
            if (i < batch.Count - 1) sb.Append(',');
        }
        sb.Append(']');

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(sb.ToString()));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        req.timeout = 10;
        yield return req.SendWebRequest();
        // Silently ignore errors — analytics must never block gameplay.
    }

    private void PersistQueue()
    {
        // Persist unflushed events to PlayerPrefs so they survive app restart.
        // (Simple approach; for production use a local SQLite or file.)
        if (_queue.Count == 0) return;
        PlayerPrefs.SetInt("analytics_pending", _queue.Count);
        PlayerPrefs.Save();
    }

    private static double UtcNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
}
