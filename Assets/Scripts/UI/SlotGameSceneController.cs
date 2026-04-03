using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the in-game slot machine screen for a single loaded game.
///
/// Responsibilities:
///   • Show the active game's name, theme, accent colour.
///   • Drive reel display (emoji symbols).
///   • BET controls: Bet 1 / Bet Max / Bet+- with multiple bet levels.
///   • AUTO-SPIN toggle.
///   • Fires <see cref="WinCelebrationUI"/> on big wins.
///   • Back button → returns to lobby.
/// </summary>
public class SlotGameSceneController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────

    [Header("Game Info")]
    [SerializeField] private TMP_Text  gameNameLabel;
    [SerializeField] private TMP_Text  gameThemeLabel;
    [SerializeField] private Image     accentBar;        // top colour stripe

    [Header("Reels  (5 reels × 3 rows — assign in order)")]
    [SerializeField] private TMP_Text[] reel0Rows;
    [SerializeField] private TMP_Text[] reel1Rows;
    [SerializeField] private TMP_Text[] reel2Rows;
    [SerializeField] private TMP_Text[] reel3Rows;
    [SerializeField] private TMP_Text[] reel4Rows;

    [Header("Reel Frame")]
    [SerializeField] private Image   reelFrameImage;
    [SerializeField] private Image   reelMaskTop;
    [SerializeField] private Image   reelMaskBottom;

    [Header("Bet Controls")]
    [SerializeField] private Button    spinButton;
    [SerializeField] private Button    betPlusButton;
    [SerializeField] private Button    betMinusButton;
    [SerializeField] private Button    betMaxButton;
    [SerializeField] private Button    betOneButton;
    [SerializeField] private Button    autoSpinButton;
    [SerializeField] private TMP_Text  betLabel;
    [SerializeField] private TMP_Text  autoSpinLabel;

    [Header("HUD Labels")]
    [SerializeField] private TMP_Text  coinsLabel;
    [SerializeField] private TMP_Text  winLabel;
    [SerializeField] private TMP_Text  jackpotLabel;
    [SerializeField] private TMP_Text  paylineLabel;
    [SerializeField] private TMP_Text  rtpLabel;

    [Header("Win Overlay")]
    [SerializeField] private WinCelebrationUI winCelebration;

    [Header("Navigation")]
    [SerializeField] private Button    backButton;

    [Header("Reel Spin FX")]
    [SerializeField] private ParticleSystem winParticles;

    [Header("Theme")]
    [SerializeField] private CasinoTheme theme;

    // ── runtime ───────────────────────────────────────────────────────────

    private SlotGameDatabase.GameDefinition _def;
    private SlotMachine _slotMachine;
    private bool        _autoSpinActive;
    private Coroutine   _autoSpinRoutine;

    // Bet level table as multiples of the game's MinBet
    private static readonly float[] BetMultipliers = { 1f, 2f, 5f, 10f, 25f, 50f, 100f, 500f };
    private int _betLevel = 0;

    // Reel symbol emoji per symbol index (matches SlotMachine.Symbol enum order)
    private static readonly string[] SymbolEmoji =
        { "🍒","🍋","🍊","🍇","🔔","📊","7️⃣","💎","⭐","🃏","🐯","🐉","🌸","🦁","🔮" };

    // ── lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _slotMachine = FindObjectOfType<SlotMachine>();
    }

    private void Start()
    {
        if (spinButton)    spinButton.onClick.AddListener(OnSpinClicked);
        if (betPlusButton) betPlusButton.onClick.AddListener(OnBetPlus);
        if (betMinusButton)betMinusButton.onClick.AddListener(OnBetMinus);
        if (betMaxButton)  betMaxButton.onClick.AddListener(OnBetMax);
        if (betOneButton)  betOneButton.onClick.AddListener(OnBetOne);
        if (autoSpinButton)autoSpinButton.onClick.AddListener(OnToggleAutoSpin);
        if (backButton)    backButton.onClick.AddListener(OnBack);

        // Subscribe to spin events
        if (_slotMachine != null)
            _slotMachine.OnSpinFinished += HandleSpinFinished;

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_slotMachine != null)
            _slotMachine.OnSpinFinished -= HandleSpinFinished;
    }

    private void Update()
    {
        // Live HUD refresh (coin changes from remote, jackpot ticker)
        RefreshCoins();
        RefreshJackpot();
    }

    // ── public: called by SlotGameLoader after scene load ────────────────

    public void ApplyGameDefinition(SlotGameDatabase.GameDefinition def)
    {
        _def      = def;
        _betLevel = 0;

        if (gameNameLabel  != null) gameNameLabel.text  = def.GameName;
        if (gameThemeLabel != null) gameThemeLabel.text = def.Theme;

        if (accentBar != null && ColorUtility.TryParseHtmlString(def.AccentColor, out Color acc))
            accentBar.color = acc;

        if (paylineLabel != null) paylineLabel.text = $"{def.PaylineCount} LINES";
        if (rtpLabel     != null) rtpLabel.text     = $"RTP {def.BaseRTP * 100f:0}%";

        // Apply bet
        ApplyBetLevel();
    }

    // ── spin handlers ─────────────────────────────────────────────────────

    private void OnSpinClicked()
    {
        if (winLabel != null) winLabel.text = "";
        _slotMachine?.Spin();
    }

    // Handles the event fired by SlotMachine (old API)
    private void HandleSpinFinished(SlotMachine.Symbol[,] result,
        long payout, SlotMachine.WinLevel winLevel)
    {
        UpdateReelDisplay(result);
        RefreshCoins();
        ShowWinText(payout, winLevel);
        TriggerWinCelebration(payout, winLevel);

        if (winParticles != null && payout > 0) winParticles.Play();
    }

    // ── bet controls ──────────────────────────────────────────────────────

    private void OnBetPlus()
    {
        _betLevel = Mathf.Min(_betLevel + 1, BetMultipliers.Length - 1);
        ApplyBetLevel();
    }

    private void OnBetMinus()
    {
        _betLevel = Mathf.Max(_betLevel - 1, 0);
        ApplyBetLevel();
    }

    private void OnBetMax()
    {
        _betLevel = BetMultipliers.Length - 1;
        ApplyBetLevel();
    }

    private void OnBetOne()
    {
        _betLevel = 0;
        ApplyBetLevel();
    }

    private void ApplyBetLevel()
    {
        if (_slotMachine == null) return;
        long minBet = _def.MinBet > 0 ? _def.MinBet : 100L;
        long bet    = (long)(minBet * BetMultipliers[_betLevel]);

        // Clamp to game max
        if (_def.MaxBet > 0)
            bet = System.Math.Min(bet, _def.MaxBet);

        // Write into SlotMachine via SetBet
        _slotMachine.SetBet(bet);

        if (betLabel != null)
            betLabel.text = $"BET  {FormatCoins(bet)}";
    }

    // ── auto-spin ─────────────────────────────────────────────────────────

    private void OnToggleAutoSpin()
    {
        _autoSpinActive = !_autoSpinActive;
        if (autoSpinLabel != null)
            autoSpinLabel.text = _autoSpinActive ? "STOP" : "AUTO";

        if (_autoSpinActive)
            _autoSpinRoutine = StartCoroutine(AutoSpinLoop());
        else if (_autoSpinRoutine != null)
            StopCoroutine(_autoSpinRoutine);
    }

    private IEnumerator AutoSpinLoop()
    {
        while (_autoSpinActive)
        {
            _slotMachine?.Spin();
            yield return new WaitForSeconds(2.2f);
        }
    }

    // ── navigation ────────────────────────────────────────────────────────

    private void OnBack()
    {
        if (_autoSpinRoutine != null) StopCoroutine(_autoSpinRoutine);
        _autoSpinActive = false;

        SlotGameLoader loader = FindObjectOfType<SlotGameLoader>();
        if (loader != null) loader.ReturnToLobby();
        else UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    // ── display helpers ───────────────────────────────────────────────────

    private void UpdateReelDisplay(SlotMachine.Symbol[,] result)
    {
        TMP_Text[][] allReels =
        {
            reel0Rows, reel1Rows, reel2Rows, reel3Rows, reel4Rows
        };

        int reelCount = result.GetLength(0);
        int rowCount  = result.GetLength(1);

        for (int r = 0; r < reelCount && r < allReels.Length; r++)
        {
            TMP_Text[] rows = allReels[r];
            if (rows == null) continue;
            for (int row = 0; row < rowCount && row < rows.Length; row++)
            {
                if (rows[row] == null) continue;
                int idx = (int)result[r, row];
                rows[row].text = idx < SymbolEmoji.Length ? SymbolEmoji[idx] : "❓";
            }
        }
    }

    private void ShowWinText(long payout, SlotMachine.WinLevel level)
    {
        if (winLabel == null) return;
        winLabel.text = level switch
        {
            SlotMachine.WinLevel.EpicWin => $"🎉 EPIC WIN!  +{FormatCoins(payout)}",
            SlotMachine.WinLevel.MegaWin => $"🔥 MEGA WIN!  +{FormatCoins(payout)}",
            SlotMachine.WinLevel.BigWin  => $"💥 BIG WIN!   +{FormatCoins(payout)}",
            SlotMachine.WinLevel.Normal  => payout > 0 ? $"+{FormatCoins(payout)}" : "Try again!",
            _                            => ""
        };
    }

    private void TriggerWinCelebration(long payout, SlotMachine.WinLevel level)
    {
        if (winCelebration == null || payout <= 0) return;
        switch (level)
        {
            case SlotMachine.WinLevel.EpicWin: winCelebration.ShowEpicWin(payout); break;
            case SlotMachine.WinLevel.MegaWin: winCelebration.ShowMegaWin(payout); break;
            case SlotMachine.WinLevel.BigWin:  winCelebration.ShowBigWin(payout);  break;
        }
    }

    private long _lastCoins   = long.MinValue;
    private long _lastJackpot = long.MinValue;

    private void RefreshCoins()
    {
        if (PlayerEconomy.Instance == null || coinsLabel == null) return;
        long coins = PlayerEconomy.Instance.Coins;
        if (coins == _lastCoins) return;
        _lastCoins     = coins;
        coinsLabel.text = $"💰 {FormatCoins(coins)}";
    }

    private void RefreshJackpot()
    {
        if (ProgressiveJackpot.Instance == null || jackpotLabel == null) return;
        long jp = ProgressiveJackpot.Instance.CurrentJackpot;
        if (jp == _lastJackpot) return;
        _lastJackpot     = jp;
        jackpotLabel.text = $"🏆 {FormatCoins(jp)}";
    }

    private void RefreshAll()
    {
        RefreshCoins();
        RefreshJackpot();
        ApplyBetLevel();
    }

    private static string FormatCoins(long amount)
    {
        if (amount >= 1_000_000_000_000L) return $"{amount / 1_000_000_000_000.0:0.##}T";
        if (amount >= 1_000_000_000L)     return $"{amount / 1_000_000_000.0:0.##}B";
        if (amount >= 1_000_000L)         return $"{amount / 1_000_000.0:0.##}M";
        if (amount >= 1_000L)             return $"{amount / 1_000.0:0.##}K";
        return amount.ToString();
    }
}
