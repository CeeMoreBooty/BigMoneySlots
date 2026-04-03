using UnityEngine;

/// <summary>
/// Singleton game manager. Bootstraps core systems on startup.
/// All field references are optional — null-checked so the game
/// runs even when fields are unassigned in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References (all optional — set in Inspector)")]
    public SlotMachine slotMachine;
    public DailyChallenges dailyChallenges;
    public DailyBonus dailyBonus;
    public UserData userData;
    public APIManager apiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (userData != null)   userData.Load();
        if (dailyBonus != null) dailyBonus.CheckAndAward();
    }

    public void OnSpinComplete(long coinsWon, SlotMachine.WinLevel winLevel)
    {
        if (userData != null)
        {
            userData.AddCoins(coinsWon);
        }

        if (dailyChallenges != null)
        {
            dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.SpinSlots, 1L);
            dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.WinCoins, coinsWon);
            dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.PlayGames, 1L);

            switch (winLevel)
            {
                case SlotMachine.WinLevel.BigWin:
                    dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.WinBigWin, 1L);
                    break;
                case SlotMachine.WinLevel.MegaWin:
                    dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.WinMegaWin, 1L);
                    break;
                case SlotMachine.WinLevel.EpicWin:
                    dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.WinEpicWin, 1L);
                    break;
            }
        }

        if (apiManager != null)
            _ = apiManager.PostSpinResult(coinsWon, (int)winLevel);
    }

    public void OnDailyBonusCollected(long coins)
    {
        if (userData != null)
            userData.AddCoins(coins);
        if (dailyChallenges != null)
            dailyChallenges.RecordProgress(DailyChallenges.ChallengeType.CollectBonus, 1L);
    }
}
