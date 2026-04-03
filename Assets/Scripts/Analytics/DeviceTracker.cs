using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Collects device fingerprint data: hardware ID, platform, OS, screen
/// resolution, timezone, and IP address (resolved via a public geo-IP API).
///
/// All data is cached in PlayerPrefs and refreshed once per calendar day.
/// The hardware ID is MD5-hashed before storage or transmission.
/// </summary>
public class DeviceTracker : MonoBehaviour
{
    public static DeviceTracker Instance { get; private set; }

    // ── Fingerprint properties ────────────────────────────────────────────────
    public string DeviceId        { get; private set; }
    public string Platform        { get; private set; }
    public string OperatingSystem { get; private set; }
    public string DeviceModel     { get; private set; }
    public string DeviceType      { get; private set; }
    public string GraphicsDevice  { get; private set; }
    public int    SystemMemoryMB  { get; private set; }
    public string ScreenRes       { get; private set; }
    public string TimeZone        { get; private set; }
    public string IpAddress       { get; private set; }
    public string Country         { get; private set; }

    private const string KeyIp      = "dt_ip";
    private const string KeyCountry = "dt_country";
    private const string KeyUpdated = "dt_updated";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CaptureHardwareInfo();
    }

    private void Start()
    {
        double lastUpdate;
        if (!double.TryParse(PlayerPrefs.GetString(KeyUpdated, "0"), out lastUpdate))
            lastUpdate = 0;

        if (UtcNow() - lastUpdate > 86400)
            StartCoroutine(FetchIpInfo());
        else
        {
            IpAddress = PlayerPrefs.GetString(KeyIp,      "unknown");
            Country   = PlayerPrefs.GetString(KeyCountry, "unknown");
        }
    }

    // ── Hardware fingerprint ──────────────────────────────────────────────────
    private void CaptureHardwareInfo()
    {
        DeviceId       = HashId(SystemInfo.deviceUniqueIdentifier);
        Platform       = Application.platform.ToString();
        OperatingSystem= SystemInfo.operatingSystem;
        DeviceModel    = SystemInfo.deviceModel;
        DeviceType     = SystemInfo.deviceType.ToString();
        GraphicsDevice = SystemInfo.graphicsDeviceName;
        SystemMemoryMB = SystemInfo.systemMemorySize;
        ScreenRes      = Screen.width + "x" + Screen.height;
        TimeZone       = System.TimeZoneInfo.Local.DisplayName;

        PlayerPrefs.SetString("device_id", DeviceId);
        PlayerPrefs.Save();

        Debug.Log($"[DeviceTracker] ID={DeviceId} Platform={Platform} OS={OperatingSystem} Screen={ScreenRes}");
    }

    // ── IP / Geo lookup ───────────────────────────────────────────────────────
    private IEnumerator FetchIpInfo()
    {
        const string url = "https://ipapi.co/json/";
        using var req = UnityWebRequest.Get(url);
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            IpAddress = ExtractField(json, "ip");
            Country   = ExtractField(json, "country_name");
        }
        else
        {
            IpAddress = "unknown";
            Country   = "unknown";
        }

        PlayerPrefs.SetString(KeyIp,      IpAddress);
        PlayerPrefs.SetString(KeyCountry, Country);
        PlayerPrefs.SetString(KeyUpdated, UtcNow().ToString(
            System.Globalization.CultureInfo.InvariantCulture));
        PlayerPrefs.Save();

        Debug.Log($"[DeviceTracker] IP={IpAddress} Country={Country}");

        // Report to analytics using JsonUtility-style safe escaping
        string payload = JsonEscape(IpAddress, Country, Platform, OperatingSystem, DeviceModel, ScreenRes);
        AnalyticsManager.Instance?.Track("device_info", payload);
    }

    // Safely builds a JSON object, escaping special characters in each field value
    private static string JsonEscape(string ip, string country, string platform,
                                     string os, string model, string screen)
    {
        return "{\"ip\":\""       + EscapeJson(ip)       +
               "\",\"country\":\"" + EscapeJson(country)  +
               "\",\"platform\":\"" + EscapeJson(platform) +
               "\",\"os\":\""      + EscapeJson(os)       +
               "\",\"model\":\""   + EscapeJson(model)    +
               "\",\"screen\":\""  + EscapeJson(screen)   + "\"}";
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string HashId(string raw)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("bms:" + raw));
        return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static string ExtractField(string json, string key)
    {
        string search = "\"" + key + "\":\"";
        int idx = json.IndexOf(search, System.StringComparison.Ordinal);
        if (idx < 0) return "unknown";
        int start = idx + search.Length;
        int end   = json.IndexOf('"', start);
        return end > start ? json.Substring(start, end - start) : "unknown";
    }

    private static double UtcNow() =>
        (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
}
