using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Standalone Daily Bonus popup: 7-day streak reward with escalating prizes.
/// Can be triggered from MainMenuController or shown as a modal on app launch.
/// </summary>
public class DailyBonusController : MonoBehaviour
{
    [Serializable]
    public class DailyReward
    {
        public int  day;
        public long coinReward;
        public Sprite icon;
    }

    [Header("Rewards (7 days)")]
    [SerializeField] private DailyReward[] rewards;

    [Header("UI")]
    [SerializeField] private GameObject    bonusPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private DaySlotUI[]   daySlots;    // 7 day display slots
    [SerializeField] private Button        claimButton;
    [SerializeField] private Button        closeButton;
    [SerializeField] private CoinDisplay   coinDisplay;
    [SerializeField] private TextMeshProUGUI claimAmountText;

    private const string STREAK_KEY   = "DailyBonusStreak";
    private const string LAST_DAY_KEY = "DailyBonusLastDay";

    private int  currentStreak;
    private int  todayRewardDay;

    private void Start()
    {
        claimButton?.onClick.AddListener(ClaimReward);
        closeButton?.onClick.AddListener(HidePanel);
        bonusPanel?.SetActive(false);
    }

    /// <summary>Call from MainMenu to check and show the daily bonus panel.</summary>
    public void CheckAndShow()
    {
        if (!GameData.CanClaimDailyBonus) return;

        LoadStreak();
        todayRewardDay = ((currentStreak) % 7) + 1;
        RefreshUI();
        bonusPanel?.SetActive(true);
    }

    private void LoadStreak()
    {
        string lastDay = PlayerPrefs.GetString(LAST_DAY_KEY, string.Empty);
        string today   = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        if (lastDay == yesterday)
            currentStreak = PlayerPrefs.GetInt(STREAK_KEY, 0) + 1;
        else if (lastDay == today)
            currentStreak = PlayerPrefs.GetInt(STREAK_KEY, 0);
        else
            currentStreak = 0; // streak broken
    }

    private void RefreshUI()
    {
        for (int i = 0; i < daySlots.Length && i < rewards.Length; i++)
        {
            DailyReward reward = rewards[i];
            DaySlotUI   slot   = daySlots[i];
            bool isToday       = (reward.day == todayRewardDay);
            bool isPast        = (reward.day < todayRewardDay);
            slot?.Setup(reward, isToday, isPast);
        }

        long todayCoins = GetTodayReward()?.coinReward ?? 1000;
        if (claimAmountText != null)
            claimAmountText.text = $"+{CoinDisplay.FormatCoins(todayCoins)} COINS";
    }

    private DailyReward GetTodayReward()
    {
        foreach (var r in rewards)
            if (r.day == todayRewardDay) return r;
        return null;
    }

    private void ClaimReward()
    {
        SoundManager.Instance?.PlayBonus();

        var reward = GetTodayReward();
        if (reward != null)
        {
            if (PlayerEconomy.Instance != null)
                PlayerEconomy.Instance.AddCoins(reward.coinReward);
            else
            {
                GameData.AddCoins(reward.coinReward);
                GameData.Save();
            }
            GameData.LastDailyBonusDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            PlayerPrefs.SetInt(STREAK_KEY, currentStreak);
            PlayerPrefs.SetString(LAST_DAY_KEY, DateTime.UtcNow.ToString("yyyy-MM-dd"));
            GameData.Save();
            coinDisplay?.AnimateTo(CoinDisplay.GetCoins());
        }

        HidePanel();
    }

    private void HidePanel()
    {
        bonusPanel?.SetActive(false);
    }
}

/// <summary>Helper component for each day slot in the 7-day calendar.</summary>
public class DaySlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Image           rewardIcon;
    [SerializeField] private GameObject      todayHighlight;
    [SerializeField] private GameObject      claimedOverlay;

    public void Setup(DailyBonusController.DailyReward reward, bool isToday, bool isPast)
    {
        if (dayText    != null) dayText.text    = $"Day {reward.day}";
        if (rewardText != null) rewardText.text = CoinDisplay.FormatCoins(reward.coinReward);
        if (rewardIcon != null && reward.icon != null) rewardIcon.sprite = reward.icon;

        todayHighlight?.SetActive(isToday);
        claimedOverlay?.SetActive(isPast);
    }
}
