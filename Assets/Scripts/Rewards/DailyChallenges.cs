using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Daily challenge system. Generates 3 challenges per day, tracks progress via
/// PlayerPrefs, and awards coins on completion.
/// Uses long values to support 5B-coin targets.
/// </summary>
public class DailyChallenges : MonoBehaviour
{
    // ── Challenge data ────────────────────────────────────────────────────────

    public enum ChallengeType { SpinCount, WinAmount, BigWinCount, JackpotHit, ReachBet }

    [Serializable]
    public class Challenge
    {
        public string        id;
        public ChallengeType type;
        public string        displayName;
        public long          target;     // long for up to 5B-coin goals
        public long          reward;
        public bool          completed;

        // Runtime progress – stored as string in PlayerPrefs for large values
        private long _progress;
        public long Progress
        {
            get => _progress;
            set => _progress = Math.Min(value, target);
        }

        public float ProgressRatio => target > 0 ? (float)Progress / target : 0f;

        public string ProgressKey => $"DailyChallenge_{id}_Progress";
        public string DoneKey     => $"DailyChallenge_{id}_Done";
        public string DateKey     => $"DailyChallenge_{id}_Date";
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    [System.Serializable]
    public class ChallengeRowUI
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI progressText;
        public Slider          progressBar;
        public Button          claimButton;
        public GameObject      completedBadge;
    }

    [Header("UI")]
    [SerializeField] private GameObject       panel;
    [SerializeField] private Button           closeButton;
    [SerializeField] private ChallengeRowUI[] rows;
    [SerializeField] private TextMeshProUGUI  resetTimerText;
    [SerializeField] private CoinDisplay      coinDisplay;

    // ── State ─────────────────────────────────────────────────────────────────

    private List<Challenge> todayChallenges = new List<Challenge>();

    // Predefined challenge pool
    private static readonly (ChallengeType type, string name, long target, long reward)[] Pool =
    {
        (ChallengeType.SpinCount,   "Spin 50 times",             50L,           1_000L),
        (ChallengeType.SpinCount,   "Spin 200 times",           200L,           5_000L),
        (ChallengeType.WinAmount,   "Win 10,000 coins",      10_000L,           2_000L),
        (ChallengeType.WinAmount,   "Win 1,000,000 coins", 1_000_000L,          50_000L),
        (ChallengeType.WinAmount,   "Win 5,000,000,000", 5_000_000_000L,     500_000L),
        (ChallengeType.BigWinCount, "Score 3 Big Wins",           3L,           5_000L),
        (ChallengeType.BigWinCount, "Score 10 Big Wins",         10L,          25_000L),
        (ChallengeType.ReachBet,    "Bet 5,000 on a spin",    5_000L,           3_000L),
        (ChallengeType.ReachBet,    "Bet max on a spin",     10_000L,          10_000L),
        (ChallengeType.JackpotHit,  "Hit the Jackpot!",           1L,         100_000L),
    };

    private void Start()
    {
        closeButton?.onClick.AddListener(Hide);
        panel?.SetActive(false);
        GenerateTodayChallenges();
    }

    private void Update()
    {
        UpdateResetTimer();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        panel?.SetActive(true);
        GenerateTodayChallenges();
        RefreshUI();
    }

    public void Hide()
    {
        panel?.SetActive(false);
        SoundManager.Instance?.PlayButtonClick();
    }

    // Called by SlotMachine / GameUIController after each relevant event
    public void OnSpin()            => IncrementChallenge(ChallengeType.SpinCount, 1);
    public void OnWin(long amount)  => IncrementChallenge(ChallengeType.WinAmount, amount);
    public void OnBigWin()          => IncrementChallenge(ChallengeType.BigWinCount, 1);
    public void OnJackpot()         => IncrementChallenge(ChallengeType.JackpotHit, 1);
    public void OnBet(long amount)  => SetChallengeIfGreater(ChallengeType.ReachBet, amount);

    // ── Challenge generation ──────────────────────────────────────────────────

    private void GenerateTodayChallenges()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        todayChallenges.Clear();

        // Use date as a seed for deterministic daily selection
        System.Random rng = new System.Random(today.GetHashCode());
        var indices = new List<int>();
        while (indices.Count < 3 && indices.Count < Pool.Length)
        {
            int idx = rng.Next(Pool.Length);
            if (!indices.Contains(idx)) indices.Add(idx);
        }

        foreach (int idx in indices)
        {
            var (type, name, target, reward) = Pool[idx];
            var challenge = new Challenge
            {
                id          = $"{today}_{idx}",
                type        = type,
                displayName = name,
                target      = target,
                reward      = reward,
            };

            // Load persisted progress
            string storedDate = PlayerPrefs.GetString(challenge.DateKey, string.Empty);
            if (storedDate == today)
            {
                challenge.Progress  = long.Parse(PlayerPrefs.GetString(challenge.ProgressKey, "0"));
                challenge.completed = PlayerPrefs.GetInt(challenge.DoneKey, 0) == 1;
            }
            else
            {
                // New day: reset
                PlayerPrefs.SetString(challenge.DateKey, today);
                PlayerPrefs.SetString(challenge.ProgressKey, "0");
                PlayerPrefs.SetInt(challenge.DoneKey, 0);
            }

            todayChallenges.Add(challenge);
        }
    }

    private void IncrementChallenge(ChallengeType type, long delta)
    {
        bool anyCompleted = false;
        foreach (var c in todayChallenges)
        {
            if (c.type != type || c.completed) continue;
            c.Progress += delta;
            SaveProgress(c);
            if (c.Progress >= c.target) anyCompleted = true;
        }
        if (anyCompleted) RefreshUI();
    }

    private void SetChallengeIfGreater(ChallengeType type, long value)
    {
        foreach (var c in todayChallenges)
        {
            if (c.type != type || c.completed) continue;
            if (value > c.Progress)
            {
                c.Progress = value;
                SaveProgress(c);
            }
        }
        RefreshUI();
    }

    private void SaveProgress(Challenge c)
    {
        PlayerPrefs.SetString(c.ProgressKey, c.Progress.ToString());
        PlayerPrefs.SetInt(c.DoneKey, c.completed ? 1 : 0);
    }

    // ── UI refresh ────────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        for (int i = 0; i < rows.Length && i < todayChallenges.Count; i++)
        {
            var c   = todayChallenges[i];
            var row = rows[i];

            if (row.nameText     != null) row.nameText.text     = $"{c.displayName} (+{CoinDisplay.FormatCoins(c.reward)})";
            if (row.progressText != null) row.progressText.text = $"{CoinDisplay.FormatCoins(c.Progress)} / {CoinDisplay.FormatCoins(c.target)}";
            if (row.progressBar  != null) row.progressBar.value = c.ProgressRatio;

            bool canClaim = c.Progress >= c.target && !c.completed;
            row.claimButton?.gameObject.SetActive(canClaim);
            row.completedBadge?.SetActive(c.completed);

            int captured = i;
            row.claimButton?.onClick.RemoveAllListeners();
            row.claimButton?.onClick.AddListener(() => ClaimChallenge(captured));
        }
    }

    private void ClaimChallenge(int index)
    {
        if (index < 0 || index >= todayChallenges.Count) return;
        var c = todayChallenges[index];
        if (c.completed || c.Progress < c.target) return;

        c.completed = true;
        SaveProgress(c);
        GameData.AddCoins(c.reward);
        GameData.Save();

        SoundManager.Instance?.PlayCoinDrop();
        coinDisplay?.AnimateTo(GameData.Coins);
        RefreshUI();
    }

    private void UpdateResetTimer()
    {
        if (resetTimerText == null) return;
        DateTime nextReset = DateTime.UtcNow.Date.AddDays(1);
        TimeSpan remaining = nextReset - DateTime.UtcNow;
        resetTimerText.text = $"Resets in {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
    }
}
