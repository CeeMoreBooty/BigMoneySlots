using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles all HTTP communication with the Railway backend.
/// Base URL is read from the BACKEND_URL environment variable at build time
/// or falls back to the value set in the Unity Inspector.
/// </summary>
public class APIManager : MonoBehaviour
{
    [Tooltip("Set this to your Railway backend URL, e.g. https://bigmoneyslots.up.railway.app")]
    [SerializeField] private string backendUrl = "https://bigmoneyslots.up.railway.app";

    private string token;

    private string BaseUrl => backendUrl.TrimEnd('/');

    // ──────────────────────────────────────────────────────────────────────────
    // Auth
    // ──────────────────────────────────────────────────────────────────────────

    public Coroutine Register(string username, string password,
        Action<bool, string> callback)
    {
        var body = new AuthRequest { username = username, password = password };
        return StartCoroutine(Post("/api/auth/register", body, (ok, json) =>
        {
            if (ok)
            {
                var resp = JsonUtility.FromJson<AuthResponse>(json);
                token = resp.token;
            }
            callback(ok, json);
        }));
    }

    public Coroutine Login(string username, string password,
        Action<bool, string> callback)
    {
        var body = new AuthRequest { username = username, password = password };
        return StartCoroutine(Post("/api/auth/login", body, (ok, json) =>
        {
            if (ok)
            {
                var resp = JsonUtility.FromJson<AuthResponse>(json);
                token = resp.token;
            }
            callback(ok, json);
        }));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Game data
    // ──────────────────────────────────────────────────────────────────────────

    public Coroutine PostSpinResult(long coinsWon, int winLevel,
        Action<bool, string> callback = null)
    {
        var body = new SpinResult { coinsWon = coinsWon, winLevel = winLevel };
        return StartCoroutine(Post("/api/game/spin", body, (ok, json) =>
            callback?.Invoke(ok, json)));
    }

    public Coroutine GetLeaderboard(Action<bool, string> callback)
    {
        return StartCoroutine(Get("/api/leaderboard", callback));
    }

    public Coroutine GetProfile(Action<bool, string> callback)
    {
        return StartCoroutine(Get("/api/user/profile", callback));
    }

    public Coroutine SyncChallenges(string challengeData,
        Action<bool, string> callback = null)
    {
        var body = new ChallengeSync { data = challengeData };
        return StartCoroutine(Post("/api/challenges/sync", body, (ok, json) =>
            callback?.Invoke(ok, json)));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // HTTP helpers
    // ──────────────────────────────────────────────────────────────────────────

    private IEnumerator Get(string path, Action<bool, string> callback)
    {
        string url = BaseUrl + path;
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success;
        callback(ok, ok ? req.downloadHandler.text : req.error);
    }

    private IEnumerator Post(string path, object body,
        Action<bool, string> callback)
    {
        string url  = BaseUrl + path;
        string json = JsonUtility.ToJson(body);
        byte[] raw  = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(raw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success;
        callback(ok, ok ? req.downloadHandler.text : req.error);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DTO types (must be serializable by JsonUtility)
    // ──────────────────────────────────────────────────────────────────────────

    [Serializable] private class AuthRequest  { public string username; public string password; }
    [Serializable] private class AuthResponse { public string token; }
    [Serializable] private class SpinResult   { public long coinsWon; public int winLevel; }
    [Serializable] private class ChallengeSync { public string data; }
}
