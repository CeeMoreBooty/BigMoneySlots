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
/// <see cref="flushIntervalSeconds"/> seconds, or when the app pauses / quits.
/// All network calls are fire-and-forget — failures are silently swallowed so
/// analytics never causes a gameplay error.
///
/// Wired automatically:
///   • Session start / end
///   • Jackpot wins (via ProgressiveJackpot.OnJackpotWon)
///   • Spin + win events (via SlotMachine.Spin() calling TrackSpin directly)
///   • Tier-up (via LoyaltySystem.OnTierUp)
///   • Scene loads (via SceneLoader)
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Flush Settings")]
    [Tooltip("Seconds between automatic batch flushes to backend.")]
    public float flushIntervalSeconds = 30f;

    [Header("Debug")]
    [Tooltip("Print every tracked event to the Unity Console.")]
    public bool logEvents = false;

    // ── Event name constants ──────────────────────────────────────────────────
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
        public const string TierUp         = "tier_up";
        public const string MilestoneReach = "milestone";
        public const string ErrorCaught    = "error_caught";
    }

    // ── Data model ────────────────────────────────────────────────────────────
    [Serializable]
    private class AnalyticsEvent
    {
        public string eventName;
        public string deviceId;
        public string sessionId;
        public long   timestamp;
        public string payload;   // compact JSON
    }

    private readonly List<AnalyticsEvent> _queue = new List<AnalyticsEvent>();
    private string _sessionId;
    private double _sessionStartTime;
    private const int MaxQueueSize = 200;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sessionId        = Guid.NewGuid().ToString("N").Substring(0, 16);
        _sessionStartTime = UtcNow();

        int count = PlayerPrefs.GetInt("analytics_session_count", 0) + 1;
        PlayerPrefs.SetInt("analytics_session_count", count);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        int sessionCount = PlayerPrefs.GetInt("analytics_session_count", 1);
        Track(Event.SessionStart,
              "{\"session\":\"" + _sessionId + "\",\"session_count\":" + sessionCount + "}");

        // Subscribe to events that Analytics tracks automatically.
        ProgressiveJackpot.OnJackpotWon += OnJackpotWon;
        LoyaltySystem.OnTierUp          += OnTierUp;
        LoyaltySystem.OnMilestoneReached += OnMilestone;

        InvokeRepeating(nameof(Flush), flushIntervalSeconds, flushIntervalSeconds);
    }

    private void OnDestroy()
    {
        ProgressiveJackpot.OnJackpotWon  -= OnJackpotWon;
        LoyaltySystem.OnTierUp           -= OnTierUp;
        LoyaltySystem.OnMilestoneReached -= OnMilestone;
    }

    // ── Public tracking API ───────────────────────────────────────────────────

    /// <summary>Track a named event with an optional JSON payload string.</summary>
    public void Track(string eventName, string jsonPayload = "{}")
    {
        if (_queue.Count >= MaxQueueSize) _queue.RemoveAt(0);

        _queue.Add(new AnalyticsEvent
        {
            eventName = eventName,
            deviceId  = DeviceTracker.Instance != null
                        ? DeviceTracker.Instance.DeviceId
                        : SystemInfo.deviceUniqueIdentifier,
            sessionId = _sessionId,
            timestamp = (long)UtcNow(),
            payload   = jsonPayload,
        });

        if (logEvents)
            Debug.Log("[Analytics] " + eventName + ": " + jsonPayload);
    }

    /// <summary>
    /// Called by SlotMachine.Spin() / SpinFree() after every spin result.
    /// </summary>
    public void TrackSpin(long bet, long win, bool isJackpot)
    {
        string evtName = isJackpot ? Event.JackpotWin : (win > 0 ? Event.Win : Event.Spin);
        Track(evtName,
              "{\"bet\":" + bet +
              ",\"win\":" + win +
              ",\"jackpot\":" + (isJackpot ? "true" : "false") + "}");
    }

    // ── Event listeners ───────────────────────────────────────────────────────
    private void OnJackpotWon(ProgressiveJackpot.JackpotTier tier, long amount, string label)
        => Track(Event.JackpotWin,
                 "{\"tier\":\"" + label + "\",\"amount\":" + amount + "}");

    private void OnTierUp(LoyaltySystem.LoyaltyTier tier)
        => Track(Event.TierUp,
                 "{\"tier\":\"" + tier + "\"}");

    private void OnMilestone(long coins, int freeSpins, int superSpins)
        => Track(Event.MilestoneReach,
                 "{\"coins\":" + coins + ",\"free_spins\":" + freeSpins + "}");

    // ── Flush ─────────────────────────────────────────────────────────────────
    private void OnApplicationPause(bool paused) { if (paused) Flush(); }
    private void OnApplicationQuit()
    {
        long duration = (long)(UtcNow() - _sessionStartTime);
        Track(Event.SessionEnd, "{\"duration\":" + duration + "}");
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
        string url = BackendClient.BaseUrl + "/api/analytics/batch";
        var sb = new StringBuilder("[");
        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            sb.Append("{\"event\":\"").Append(e.eventName)
              .Append("\",\"device\":\"").Append(e.deviceId)
              .Append("\",\"session\":\"").Append(e.sessionId)
              .Append("\",\"ts\":").Append(e.timestamp)
              .Append(",\"data\":").Append(e.payload).Append('}');
            if (i < batch.Count - 1) sb.Append(',');
        }
        sb.Append(']');

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(sb.ToString()));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        req.timeout = 10;
        yield return req.SendWebRequest();
        // Silently ignore errors — analytics must never block gameplay.
    }

    private void PersistQueue()
    {
        if (_queue.Count == 0) return;
        PlayerPrefs.SetInt("analytics_pending", _queue.Count);
        PlayerPrefs.Save();
    }

    private static double UtcNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
}
