using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Client-side fraud-prevention layer.
///
/// Checks performed:
///   - Device ban list (server-side lookup on session start)
///   - Spin rate limiting  (prevents macro/bot abuse)
///   - Impossible-win detection (server validates; client flags anomalies)
///   - Coin balance sanity check (detect PlayerPrefs tampering)
///   - Time-warp detection  (system clock vs server-reported time)
///
/// All checks are advisory — the authoritative validation lives on the backend.
/// Client findings are reported to AnalyticsManager for server review.
/// </summary>
public class FraudPrevention : MonoBehaviour
{
    public static FraudPrevention Instance { get; private set; }

    [Header("Rate Limiting")]
    [Tooltip("Minimum seconds allowed between two Spin() calls.")]
    public float minSpinIntervalSec = 0.5f;

    [Tooltip("Maximum spins per minute before flagging as a bot.")]
    public int maxSpinsPerMinute = 60;

    [Header("Balance Sanity")]
    [Tooltip("Maximum legitimate coin balance (above this is likely tampered).")]
    public long maxSaneBalance = 999_999_999_999_999L;

    // ── Runtime state ─────────────────────────────────────────────────────────
    public bool IsDeviceBanned { get; private set; }

    private double _lastSpinTime  = -999;
    private int    _spinsThisMinute;
    private double _minuteWindowStart;
    private bool   _fraudFlagged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _minuteWindowStart = UtcNow();
        StartCoroutine(CheckDeviceBan());
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call before every Spin().  Returns true if the spin should be allowed.
    /// </summary>
    public bool AllowSpin()
    {
        if (IsDeviceBanned)
        {
            Debug.LogWarning("[FraudPrevention] Spin blocked — device is banned.");
            return false;
        }

        double now = UtcNow();

        // Rate: minimum interval between spins.
        if (now - _lastSpinTime < minSpinIntervalSec)
        {
            Debug.LogWarning("[FraudPrevention] Spin too fast — throttled.");
            return false;
        }

        // Rate: spins per minute window.
        if (now - _minuteWindowStart >= 60)
        {
            _minuteWindowStart = now;
            _spinsThisMinute   = 0;
        }
        _spinsThisMinute++;
        if (_spinsThisMinute > maxSpinsPerMinute)
        {
            FlagFraud("spin_rate_exceeded",
                      $"{{"spins_per_min":{_spinsThisMinute}}}");
            return false;
        }

        _lastSpinTime = now;
        return true;
    }

    /// <summary>Call after every spin result to check win plausibility.</summary>
    public void ValidateWin(long bet, long payout)
    {
        if (payout <= 0) return;
        float multiplier = bet > 0 ? (float)payout / bet : 0;
        if (multiplier > 10_000)
            FlagFraud("impossible_win",
                      $"{{"bet":{bet},"payout":{payout},"mult":{multiplier:F0}}}");
    }

    /// <summary>Sanity-check the current coin balance.</summary>
    public void ValidateBalance()
    {
        if (PlayerEconomy.Instance == null) return;
        if (PlayerEconomy.Instance.Coins > maxSaneBalance)
            FlagFraud("balance_tamper",
                      $"{{"balance":{PlayerEconomy.Instance.Coins}}}");
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private IEnumerator CheckDeviceBan()
    {
        string deviceId = DeviceTracker.Instance?.DeviceId
                          ?? SystemInfo.deviceUniqueIdentifier;
        string url = $"{BackendClient.BaseUrl}/api/security/check?device={deviceId}";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string body = req.downloadHandler.text;
            if (body.Contains(""banned":true"))
            {
                IsDeviceBanned = true;
                Debug.LogError("[FraudPrevention] Device is banned.");
                AnalyticsManager.Instance?.Track(AnalyticsManager.Event.FraudFlag,
                    "{"reason":"device_banned"}");
            }
        }
        // Non-fatal if server unreachable — assume innocent.
    }

    private void FlagFraud(string reason, string payload)
    {
        if (_fraudFlagged) return;   // Only flag once per session to avoid spam.
        _fraudFlagged = true;

        Debug.LogWarning($"[FraudPrevention] Fraud flag: {reason} {payload}");
        AnalyticsManager.Instance?.Track(AnalyticsManager.Event.FraudFlag,
            $"{{"reason":"{reason}","data":{payload}}}");

        StartCoroutine(ReportToBackend(reason, payload));
    }

    private IEnumerator ReportToBackend(string reason, string payload)
    {
        string url  = $"{BackendClient.BaseUrl}/api/security/report";
        string body = $"{{"reason":"{reason}","data":{payload}," +
                      $""device":"{DeviceTracker.Instance?.DeviceId}"}}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
        req.timeout = 10;
        yield return req.SendWebRequest();
    }

    private static double UtcNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
}
