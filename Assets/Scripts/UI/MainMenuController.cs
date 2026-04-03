using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Controls the Main Menu scene: coin display, daily bonus, navigation.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CoinDisplay coinDisplay;
    [SerializeField] private TextMeshProUGUI playerLevelText;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button dailyBonusButton;
    [SerializeField] private Button leaderboardButton;

    [Header("Daily Bonus")]
    [SerializeField] private GameObject dailyBonusReadyIndicator;
    [SerializeField] private TextMeshProUGUI dailyBonusTimerText;
    [SerializeField] private long dailyBonusAmount = 5000;

    private void Start()
    {
        SoundManager.Instance?.PlayMenuMusic();

        RefreshUI();

        playButton?.onClick.AddListener(OnPlay);
        shopButton?.onClick.AddListener(OnShop);
        settingsButton?.onClick.AddListener(OnSettings);
        dailyBonusButton?.onClick.AddListener(OnDailyBonus);
        leaderboardButton?.onClick.AddListener(OnLeaderboard);
    }

    private void Update()
    {
        UpdateDailyBonusTimer();
    }

    private void RefreshUI()
    {
        coinDisplay?.Refresh();

        if (playerLevelText != null)
            playerLevelText.text = $"Level {GameData.Level}";

        bool canClaim = GameData.CanClaimDailyBonus;
        if (dailyBonusReadyIndicator != null)
            dailyBonusReadyIndicator.SetActive(canClaim);
        if (dailyBonusButton != null)
            dailyBonusButton.interactable = canClaim;
    }

    private void UpdateDailyBonusTimer()
    {
        if (dailyBonusTimerText == null) return;
        if (GameData.CanClaimDailyBonus)
        {
            dailyBonusTimerText.text = "READY!";
            return;
        }

        DateTime nextBonus = DateTime.UtcNow.Date.AddDays(1);
        TimeSpan remaining = nextBonus - DateTime.UtcNow;
        dailyBonusTimerText.text = $"{remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    private void OnPlay()
    {
        SoundManager.Instance?.PlayButtonClick();
        UIManager.Instance?.GoToGame();
    }

    private void OnShop()
    {
        SoundManager.Instance?.PlayButtonClick();
        UIManager.Instance?.GoToShop();
    }

    private void OnSettings()
    {
        SoundManager.Instance?.PlayButtonClick();
        UIManager.Instance?.GoToSettings();
    }

    private void OnDailyBonus()
    {
        if (!GameData.CanClaimDailyBonus) return;

        SoundManager.Instance?.PlayBonus();
        GameData.AddCoins(dailyBonusAmount);
        GameData.LastDailyBonusDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        GameData.Save();
        RefreshUI();
    }

    private void OnLeaderboard()
    {
        SoundManager.Instance?.PlayButtonClick();
        // TODO: open leaderboard panel / social integration
        Debug.Log("Leaderboard button pressed");
    }
}
