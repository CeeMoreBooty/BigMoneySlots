using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Client-side fraud-prevention layer.
///
/// Checks:
///   • Device ban list  (server lookup on session start)
///   • Spin rate limiting (prevents macro / bot abuse)
///   • Impossible-win detection
///   • Coin balance sanity (detect PlayerPrefs tampering)
///
/// Called from SlotMachine.Spin() before and after every spin.
/// All checks are advisory — authoritative validation lives on the backend.
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
    [Tooltip("Maximum coin balance considered legitimate.")]
    public long maxSaneBalance = 999_999_999_999_999L;

    // ── Runtime state ─────────────────────────────────────────────────────────
    public bool IsDeviceBanned { get; private set; }

    private double _lastSpinTime     = -999;
    private int    _spinsThisMinute;
    private double _minuteWindowStart;
    private bool   _fraudFlagged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
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
    /// Called before every Spin().  Returns true if the spin should proceed.
    /// </summary>
    public bool AllowSpin()
    {
        if (IsDeviceBanned)
        {
            Debug.LogWarning("[FraudPrevention] Spin blocked — device is banned.");
            return false;
        }

        double now = UtcNow();

        // Minimum interval between spins.
        if (now - _lastSpinTime < minSpinIntervalSec)
        {
            Debug.LogWarning("[FraudPrevention] Spin throttled — too fast.");
            return false;
        }

        // Spins-per-minute rolling window.
        if (now - _minuteWindowStart >= 60.0)
        {
            _minuteWindowStart = now;
            _spinsThisMinute   = 0;
        }
        _spinsThisMinute++;
        if (_spinsThisMinute > maxSpinsPerMinute)
        {
            FlagFraud("spin_rate_exceeded",
                      "{\"spins_per_min\":" + _spinsThisMinute + "}");
            return false;
        }

        _lastSpinTime = now;
        return true;
    }

    /// <summary>Call after a winning spin to check win plausibility.</summary>
    public void ValidateWin(long bet, long payout)
    {
        if (payout <= 0) return;
        float mult = bet > 0 ? (float)payout / bet : 0f;
        if (mult > 10_000f)
            FlagFraud("impossible_win",
                      "{\"bet\":" + bet + ",\"payout\":" + payout +
                      ",\"mult\":" + (int)mult + "}");
    }

    /// <summary>Check that the current coin balance hasn't been tampered with.</summary>
    public void ValidateBalance()
    {
        if (PlayerEconomy.Instance == null) return;
        if (PlayerEconomy.Instance.Coins > maxSaneBalance)
            FlagFraud("balance_tamper",
                      "{\"balance\":" + PlayerEconomy.Instance.Coins + "}");
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private IEnumerator CheckDeviceBan()
    {
        string deviceId = DeviceTracker.Instance != null
                          ? DeviceTracker.Instance.DeviceId
                          : SystemInfo.deviceUniqueIdentifier;

        string url = BackendClient.BaseUrl + "/api/security/check?device=" + deviceId;
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string body = req.downloadHandler.text;
            if (body.Contains("banned") && body.Contains("true"))
            {
                IsDeviceBanned = true;
                Debug.LogError("[FraudPrevention] This device is banned.");
                AnalyticsManager.Instance?.Track(AnalyticsManager.Event.FraudFlag,
                    "{\"reason\":\"device_banned\"}");
            }
        }
        // Non-fatal if server unreachable — assume innocent client.
    }

    private void FlagFraud(string reason, string payload)
    {
        if (_fraudFlagged) return;
        _fraudFlagged = true;

        Debug.LogWarning("[FraudPrevention] Fraud flag: " + reason + " " + payload);
        AnalyticsManager.Instance?.Track(AnalyticsManager.Event.FraudFlag,
            "{\"reason\":\"" + reason + "\",\"data\":" + payload + "}");

        StartCoroutine(ReportToBackend(reason, payload));
    }

    private IEnumerator ReportToBackend(string reason, string payload)
    {
        string url  = BackendClient.BaseUrl + "/api/security/report";
        string devId = DeviceTracker.Instance != null
                       ? DeviceTracker.Instance.DeviceId : "unknown";
        string body = "{\"reason\":\"" + reason + "\",\"data\":" + payload +
                      ",\"device\":\"" + devId + "\"}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + BackendClient.AuthToken);
        req.timeout = 10;
        yield return req.SendWebRequest();
    }

    private static double UtcNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
}
