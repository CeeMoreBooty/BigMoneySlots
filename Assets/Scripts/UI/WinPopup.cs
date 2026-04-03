using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays win amounts with animated text and particle effects.
/// Supports Normal Win, Big Win, Mega Win, and Jackpot tiers.
/// </summary>
public class WinPopup : MonoBehaviour
{
    public enum WinTier { Normal, BigWin, MegaWin, Jackpot }

    [Header("Panel References")]
    [SerializeField] private GameObject normalWinPanel;
    [SerializeField] private GameObject bigWinPanel;
    [SerializeField] private GameObject megaWinPanel;
    [SerializeField] private GameObject jackpotPanel;

    [Header("Text Labels")]
    [SerializeField] private TextMeshProUGUI normalWinAmountText;
    [SerializeField] private TextMeshProUGUI bigWinAmountText;
    [SerializeField] private TextMeshProUGUI megaWinAmountText;
    [SerializeField] private TextMeshProUGUI jackpotAmountText;

    [Header("Particles")]
    [SerializeField] private ParticleSystem normalWinParticles;
    [SerializeField] private ParticleSystem bigWinParticles;
    [SerializeField] private ParticleSystem jackpotParticles;

    [Header("Timing")]
    [SerializeField] private float normalDisplayTime = 1.5f;
    [SerializeField] private float bigWinDisplayTime  = 3f;
    [SerializeField] private float jackpotDisplayTime = 5f;

    [Header("Thresholds (× bet)")]
    [SerializeField] private int bigWinThreshold  = 10;
    [SerializeField] private int megaWinThreshold = 25;

    private Coroutine displayCoroutine;

    private void Awake()
    {
        HideAll();
    }

    public void ShowWin(long winAmount, long betAmount)
    {
        WinTier tier = DetermineTier(winAmount, betAmount);
        ShowTier(tier, winAmount);
    }

    public void ShowJackpot(long jackpotAmount)
    {
        ShowTier(WinTier.Jackpot, jackpotAmount);
    }

    private WinTier DetermineTier(long win, long bet)
    {
        if (bet <= 0) return WinTier.Normal;
        long ratio = win / bet;
        if (ratio >= megaWinThreshold) return WinTier.MegaWin;
        if (ratio >= bigWinThreshold)  return WinTier.BigWin;
        return WinTier.Normal;
    }

    private void ShowTier(WinTier tier, long amount)
    {
        if (displayCoroutine != null) StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(DisplayRoutine(tier, amount));
    }

    private IEnumerator DisplayRoutine(WinTier tier, long amount)
    {
        HideAll();

        float displayTime = tier switch
        {
            WinTier.BigWin  => bigWinDisplayTime,
            WinTier.MegaWin => bigWinDisplayTime,
            WinTier.Jackpot => jackpotDisplayTime,
            _               => normalDisplayTime,
        };

        switch (tier)
        {
            case WinTier.Normal:
                Activate(normalWinPanel, normalWinAmountText, amount);
                normalWinParticles?.Play();
                break;
            case WinTier.BigWin:
                Activate(bigWinPanel, bigWinAmountText, amount);
                bigWinParticles?.Play();
                break;
            case WinTier.MegaWin:
                Activate(megaWinPanel, megaWinAmountText, amount);
                bigWinParticles?.Play();
                break;
            case WinTier.Jackpot:
                Activate(jackpotPanel, jackpotAmountText, amount);
                jackpotParticles?.Play();
                break;
        }

        yield return new WaitForSeconds(displayTime);
        HideAll();
    }

    private void Activate(GameObject panel, TextMeshProUGUI label, long amount)
    {
        if (panel != null) panel.SetActive(true);
        if (label  != null) label.text = CoinDisplay.FormatCoins(amount);
    }

    private void HideAll()
    {
        normalWinPanel?.SetActive(false);
        bigWinPanel?.SetActive(false);
        megaWinPanel?.SetActive(false);
        jackpotPanel?.SetActive(false);
    }
}
