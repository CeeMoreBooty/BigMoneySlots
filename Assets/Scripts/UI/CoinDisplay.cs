using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Animates the coin counter display and keeps it in sync with the active coin store.
/// Reads from <see cref="PlayerEconomy"/> when available, falling back to <see cref="GameData"/>.
/// </summary>
public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Animator coinAnimator;
    [SerializeField] private float countDuration = 1.2f;

    private static readonly int PulseTrigger = Animator.StringToHash("Pulse");

    private long displayedValue;
    private Coroutine countCoroutine;

    private void OnEnable()
    {
        displayedValue = GetCoins();
        RefreshText(displayedValue);
    }

    /// <summary>Call to animate from current displayed value to newAmount.</summary>
    public void AnimateTo(long newAmount)
    {
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(CountUp(displayedValue, newAmount));
    }

    /// <summary>Immediately set display to the current player coin balance.</summary>
    public void Refresh()
    {
        displayedValue = GetCoins();
        RefreshText(displayedValue);
    }

    /// <summary>
    /// Returns the authoritative coin balance: <see cref="PlayerEconomy"/> when present,
    /// otherwise <see cref="GameData"/> (legacy fallback).
    /// </summary>
    public static long GetCoins() =>
        PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Coins : GameData.Coins;

    private IEnumerator CountUp(long from, long to)
    {
        if (coinAnimator != null) coinAnimator.SetTrigger(PulseTrigger);

        float elapsed = 0f;
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countDuration);
            long current = (long)Mathf.Lerp(from, to, t);
            RefreshText(current);
            yield return null;
        }

        displayedValue = to;
        RefreshText(displayedValue);
    }

    private void RefreshText(long value)
    {
        if (coinText != null)
            coinText.text = FormatCoins(value);
    }

    public static string FormatCoins(long amount)
    {
        if (amount >= 1_000_000_000L) return $"{amount / 1_000_000_000.0:0.##}B";
        if (amount >= 1_000_000L)     return $"{amount / 1_000_000.0:0.##}M";
        if (amount >= 1_000L)         return $"{amount / 1_000.0:0.##}K";
        return amount.ToString("N0");
    }
}
