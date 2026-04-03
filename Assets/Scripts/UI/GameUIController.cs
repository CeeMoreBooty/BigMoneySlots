using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Master UI controller for the Game scene.
/// Wires spin button, bet controls, coin display, jackpot display,
/// free spins indicator, and the win popup together.
/// </summary>
public class GameUIController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private SlotMachine slotMachine;
    [SerializeField] private BetController betController;

    [Header("Coin & Jackpot Display")]
    [SerializeField] private CoinDisplay coinDisplay;
    [SerializeField] private TextMeshProUGUI jackpotText;
    [SerializeField] private Animator jackpotAnimator;

    [Header("Spin Controls")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button autoSpinButton;
    [SerializeField] private TextMeshProUGUI spinButtonText;

    [Header("Free Spins")]
    [SerializeField] private GameObject freeSpinsPanel;
    [SerializeField] private TextMeshProUGUI freeSpinsText;

    [Header("Win Popup")]
    [SerializeField] private WinPopup winPopup;

    [Header("Navigation")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button paytableButton;

    [Header("Auto Spin")]
    [SerializeField] private int autoSpinCount = 10;

    private bool autoSpinActive;
    private int  autoSpinsLeft;
    private static readonly int JackpotPulse = Animator.StringToHash("Pulse");

    private void Start()
    {
        SoundManager.Instance?.PlayGameMusic();

        // Bind slot machine events
        slotMachine.OnSpinStart  += HandleSpinStart;
        slotMachine.OnSpinEnd    += HandleSpinEnd;
        slotMachine.OnWin        += HandleWin;
        slotMachine.OnJackpot    += HandleJackpot;
        slotMachine.OnFreeSpins  += HandleFreeSpins;

        // Bind buttons
        spinButton?.onClick.AddListener(OnSpinPressed);
        autoSpinButton?.onClick.AddListener(OnAutoSpinPressed);
        homeButton?.onClick.AddListener(() => { StopAutoSpin(); UIManager.Instance?.GoToMainMenu(); });
        settingsButton?.onClick.AddListener(() => UIManager.Instance?.GoToSettings());
        paytableButton?.onClick.AddListener(() => UIManager.Instance?.GoToPaytable());

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (slotMachine != null)
        {
            slotMachine.OnSpinStart  -= HandleSpinStart;
            slotMachine.OnSpinEnd    -= HandleSpinEnd;
            slotMachine.OnWin        -= HandleWin;
            slotMachine.OnJackpot    -= HandleJackpot;
            slotMachine.OnFreeSpins  -= HandleFreeSpins;
        }
    }

    // ── Spin ─────────────────────────────────────────────────────────────────

    private void OnSpinPressed()
    {
        SoundManager.Instance?.PlayButtonClick();
        TrySpin();
    }

    private void OnAutoSpinPressed()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (autoSpinActive)
            StopAutoSpin();
        else
            StartAutoSpin();
    }

    private void TrySpin()
    {
        long bet = betController?.CurrentBet ?? 500L;
        if (!slotMachine.TrySpin(bet))
        {
            SoundManager.Instance?.PlayError();
            if (GameData.Coins < bet)
                ShowInsufficientFundsMessage();
        }
    }

    private void StartAutoSpin()
    {
        autoSpinActive = true;
        autoSpinsLeft  = autoSpinCount;
        StartCoroutine(AutoSpinRoutine());
        if (autoSpinButton != null) autoSpinButton.GetComponentInChildren<TextMeshProUGUI>()
            .text = "STOP AUTO";
    }

    private void StopAutoSpin()
    {
        autoSpinActive = false;
        autoSpinsLeft  = 0;
        if (autoSpinButton != null) autoSpinButton.GetComponentInChildren<TextMeshProUGUI>()
            .text = "AUTO SPIN";
    }

    private IEnumerator AutoSpinRoutine()
    {
        while (autoSpinActive && autoSpinsLeft > 0)
        {
            if (!slotMachine.IsSpinning)
            {
                TrySpin();
                autoSpinsLeft--;
                if (autoSpinsLeft <= 0) StopAutoSpin();
            }
            yield return new WaitForSeconds(0.1f);
        }
        StopAutoSpin();
    }

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleSpinStart()
    {
        SetSpinButtonEnabled(false);
    }

    private void HandleSpinEnd()
    {
        SetSpinButtonEnabled(true);
        coinDisplay?.AnimateTo(GameData.Coins);
        RefreshJackpot();
        RefreshFreeSpins();
    }

    private void HandleWin(long amount)
    {
        winPopup?.ShowWin(amount, betController?.CurrentBet ?? 500L);
    }

    private void HandleJackpot(long amount)
    {
        winPopup?.ShowJackpot(amount);
    }

    private void HandleFreeSpins()
    {
        RefreshFreeSpins();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        coinDisplay?.Refresh();
        RefreshJackpot();
        RefreshFreeSpins();
    }

    private void RefreshJackpot()
    {
        if (jackpotText != null)
        {
            jackpotText.text = $"JACKPOT\n{CoinDisplay.FormatCoins(slotMachine.CurrentJackpot)}";
            jackpotAnimator?.SetTrigger(JackpotPulse);
        }
    }

    private void RefreshFreeSpins()
    {
        bool hasFree = slotMachine.HasFreeSpins;
        freeSpinsPanel?.SetActive(hasFree);
        if (freeSpinsText != null && hasFree)
            freeSpinsText.text = $"FREE SPINS: {slotMachine.FreeSpinsLeft}";
    }

    private void SetSpinButtonEnabled(bool enabled)
    {
        if (spinButton != null) spinButton.interactable = enabled;
        if (spinButtonText != null) spinButtonText.text = enabled ? "SPIN" : "...";
    }

    private void ShowInsufficientFundsMessage()
    {
        Debug.Log("Not enough coins! Go to the shop.");
        // TODO: show a popup nudging the player to the shop
    }
}
