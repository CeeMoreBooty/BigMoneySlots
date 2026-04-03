using UnityEngine;

/// <summary>
/// Shared backend base URL used by all Unity REST clients.
/// Change BaseUrl to your deployed server address before building.
/// </summary>
public static class BackendClient
{
#if UNITY_EDITOR
    public const string BaseUrl = "http://localhost:3000";
#else
    public const string BaseUrl = "https://api.bigmoneyslots.com";  // TODO: replace with real domain
#endif

    public static string AuthToken =>
        PlayerPrefs.GetString("auth_token", "");
}
