using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// REST API client for leaderboard and player sync.
/// Configure the base URL via the Inspector or PlayerPrefs.
/// Supports both the local Node/Express backend and Railway deployment.
/// </summary>
public class BackendClient : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string baseUrl = "https://bigmoneyslots.up.railway.app/api";
    [SerializeField] private float  requestTimeout = 10f;

    // ── Models ────────────────────────────────────────────────────────────────

    [Serializable]
    private class LeaderboardResponse
    {
        public List<LeaderboardController.LeaderboardEntry> entries;
    }

    [Serializable]
    private class ScorePayload
    {
        public string playerName;
        public long   coins;
        public int    level;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Fetch top N players from the leaderboard.</summary>
    public IEnumerator GetLeaderboard(
        int limit,
        Action<List<LeaderboardController.LeaderboardEntry>> onSuccess,
        Action<string> onError)
    {
        string url = $"{baseUrl}/leaderboard?limit={limit}";
        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = (int)requestTimeout;
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<LeaderboardResponse>(req.downloadHandler.text);
                onSuccess?.Invoke(response?.entries ?? new List<LeaderboardController.LeaderboardEntry>());
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Parse error: {ex.Message}");
            }
        }
    }

    /// <summary>Submit/update the local player's score.</summary>
    public IEnumerator SubmitScore(Action<bool> onComplete = null)
    {
        var payload = new ScorePayload
        {
            playerName = PlayerPrefs.GetString("PlayerName", "Player"),
            coins      = GameData.Coins,
            level      = GameData.Level,
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (var req = new UnityWebRequest($"{baseUrl}/leaderboard", "POST"))
        {
            req.uploadHandler   = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout         = (int)requestTimeout;
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            bool success = req.result == UnityWebRequest.Result.Success;
            if (!success) Debug.LogWarning($"[BackendClient] SubmitScore failed: {req.error}");
            onComplete?.Invoke(success);
        }
    }

    /// <summary>Try submitting the score silently in the background.</summary>
    public void SubmitScoreSilent() => StartCoroutine(SubmitScore());

    // ── Lifetime helpers ──────────────────────────────────────────────────────

    private void OnApplicationPause(bool paused)
    {
        if (paused) SubmitScoreSilent();
    }

    private void OnApplicationQuit()
    {
        SubmitScoreSilent();
    }
}
