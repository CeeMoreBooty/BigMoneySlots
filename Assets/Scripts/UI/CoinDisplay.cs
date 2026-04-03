using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Animates the coin counter display and keeps it in sync with GameData.Coins.
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
        displayedValue = GameData.Coins;
        RefreshText(displayedValue);
    }

    /// <summary>Call to animate from current displayed value to newAmount.</summary>
    public void AnimateTo(long newAmount)
    {
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(CountUp(displayedValue, newAmount));
    }

    /// <summary>Immediately set display to current GameData.Coins.</summary>
    public void Refresh()
    {
        displayedValue = GameData.Coins;
        RefreshText(displayedValue);
    }

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
