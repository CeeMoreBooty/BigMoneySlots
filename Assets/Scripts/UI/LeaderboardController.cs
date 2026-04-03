using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays top players fetched from the backend leaderboard API.
/// Falls back to a local high-score if the network is unavailable.
/// </summary>
public class LeaderboardController : MonoBehaviour
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public int    rank;
        public string playerName;
        public long   coins;
    }

    [System.Serializable]
    public class LeaderboardRowUI
    {
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI coinsText;
        public GameObject      selfHighlight;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     closeButton;
    [SerializeField] private Button     refreshButton;

    [Header("Content")]
    [SerializeField] private Transform      rowContainer;
    [SerializeField] private GameObject     rowPrefab;
    [SerializeField] private int            displayCount = 10;

    [Header("Status")]
    [SerializeField] private GameObject         loadingSpinner;
    [SerializeField] private TextMeshProUGUI    statusText;

    [Header("My Rank")]
    [SerializeField] private TextMeshProUGUI    myRankText;
    [SerializeField] private TextMeshProUGUI    myCoinsText;

    private BackendAPIClient backend;
    private string localPlayerName;

    private void Start()
    {
        backend         = FindObjectOfType<BackendAPIClient>() ?? gameObject.AddComponent<BackendAPIClient>();
        localPlayerName = PlayerPrefs.GetString("PlayerName", "You");

        closeButton?.onClick.AddListener(Hide);
        refreshButton?.onClick.AddListener(Refresh);
        panel?.SetActive(false);
    }

    public void Show()
    {
        panel?.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        panel?.SetActive(false);
        SoundManager.Instance?.PlayButtonClick();
    }

    public void Refresh()
    {
        StartCoroutine(LoadLeaderboard());
    }

    private IEnumerator LoadLeaderboard()
    {
        SetLoading(true);

        yield return backend.GetLeaderboard(displayCount, OnLeaderboardLoaded, OnLeaderboardError);
    }

    private void OnLeaderboardLoaded(List<LeaderboardEntry> entries)
    {
        SetLoading(false);

        // Clear existing rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        int myRank = -1;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var go    = Instantiate(rowPrefab, rowContainer);
            var row   = go.GetComponent<LeaderboardRowUI>();

            if (row != null)
            {
                bool isMe = entry.playerName == localPlayerName;
                if (row.rankText  != null) row.rankText.text  = $"#{entry.rank}";
                if (row.nameText  != null) row.nameText.text  = entry.playerName;
                if (row.coinsText != null) row.coinsText.text = CoinDisplay.FormatCoins(entry.coins);
                row.selfHighlight?.SetActive(isMe);
                if (isMe) myRank = entry.rank;
            }
        }

        // My rank bar
        if (myRankText  != null) myRankText.text  = myRank > 0 ? $"Your Rank: #{myRank}" : "Unranked";
        if (myCoinsText != null) myCoinsText.text = CoinDisplay.FormatCoins(GameData.Coins);
    }

    private void OnLeaderboardError(string error)
    {
        SetLoading(false);
        if (statusText != null) statusText.text = $"Could not load leaderboard.\n{error}";
    }

    private void SetLoading(bool loading)
    {
        loadingSpinner?.SetActive(loading);
        if (statusText != null) statusText.text = loading ? "Loading..." : string.Empty;
        refreshButton?.gameObject.SetActive(!loading);
    }
}
