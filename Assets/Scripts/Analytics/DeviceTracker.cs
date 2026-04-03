using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Collects device fingerprint data: hardware ID, platform, OS, screen,
/// timezone, and the player's IP address (resolved server-side or via a
/// free geo-IP API).
///
/// All data is cached in PlayerPrefs and refreshed once per session.
/// No PII beyond a hashed device identifier is transmitted.
/// </summary>
public class DeviceTracker : MonoBehaviour
{
    public static DeviceTracker Instance { get; private set; }

    // ── Fingerprint properties ────────────────────────────────────────────────
    public string DeviceId       { get; private set; }   // hashed unique id
    public string Platform       { get; private set; }
    public string OperatingSystem{ get; private set; }
    public string DeviceModel    { get; private set; }
    public string DeviceType     { get; private set; }
    public string GraphicsDevice { get; private set; }
    public int    SystemMemoryMB { get; private set; }
    public string ScreenRes      { get; private set; }
    public string TimeZone       { get; private set; }
    public string IpAddress      { get; private set; }
    public string Country        { get; private set; }

    private const string KeyIp      = "dt_ip";
    private const string KeyCountry = "dt_country";
    private const string KeyUpdated = "dt_updated";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CaptureHardwareInfo();
    }

    private void Start()
    {
        // Refresh IP once per session (or if cache is older than 1 day).
        double lastUpdate = double.TryParse(PlayerPrefs.GetString(KeyUpdated, "0"),
            out double v) ? v : 0;
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
        // Hash the device ID so we never transmit the raw hardware identifier.
        string rawId  = SystemInfo.deviceUniqueIdentifier;
        DeviceId      = HashId(rawId);
        Platform      = Application.platform.ToString();
        OperatingSystem = SystemInfo.operatingSystem;
        DeviceModel   = SystemInfo.deviceModel;
        DeviceType    = SystemInfo.deviceType.ToString();
        GraphicsDevice= SystemInfo.graphicsDeviceName;
        SystemMemoryMB= SystemInfo.systemMemorySize;
        ScreenRes     = $"{Screen.width}x{Screen.height}";
        TimeZone      = System.TimeZoneInfo.Local.DisplayName;

        // Persist hashed ID so fraud prevention can flag new devices.
        PlayerPrefs.SetString("device_id", DeviceId);
        PlayerPrefs.Save();

        Debug.Log($"[DeviceTracker] DeviceId={DeviceId} Platform={Platform} " +
                  $"OS={OperatingSystem} Screen={ScreenRes} TZ={TimeZone}");
    }

    // ── IP / Geo lookup ───────────────────────────────────────────────────────
    private IEnumerator FetchIpInfo()
    {
        // ipapi.co returns plain-text IP; free, no API key required.
        const string ipUrl = "https://ipapi.co/json/";
        using var req = UnityWebRequest.Get(ipUrl);
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // Minimal JSON parse without a full JSON library.
            string json = req.downloadHandler.text;
            IpAddress = ExtractField(json, "ip");
            Country   = ExtractField(json, "country_name");
        }
        else
        {
            // Non-fatal: leave as unknown.
            IpAddress = "unknown";
            Country   = "unknown";
        }

        PlayerPrefs.SetString(KeyIp,      IpAddress);
        PlayerPrefs.SetString(KeyCountry, Country);
        PlayerPrefs.SetString(KeyUpdated, UtcNow().ToString());
        PlayerPrefs.Save();

        Debug.Log($"[DeviceTracker] IP={IpAddress} Country={Country}");

        // Report to analytics.
        AnalyticsManager.Instance?.Track("device_info",
            $"{{"ip":"{IpAddress}","country":"{Country}"," +
            $""platform":"{Platform}","os":"{OperatingSystem}"," +
            $""model":"{DeviceModel}","screen":"{ScreenRes}"}}");
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
        string search = $""{key}":"";
        int idx = json.IndexOf(search, System.StringComparison.Ordinal);
        if (idx < 0) return "unknown";
        int start = idx + search.Length;
        int end   = json.IndexOf('"', start);
        return end > start ? json.Substring(start, end - start) : "unknown";
    }

    private static double UtcNow() =>
        (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
}
