using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Jackpot UI — displays all 5 progressive jackpot tier pools in real-time.
/// </summary>
public class JackpotUI : MonoBehaviour
{
    [System.Serializable]
    public class TierDisplay
    {
        public ProgressiveJackpot.JackpotTier tier;
        public TMP_Text poolText;
        public TMP_Text chanceText;   // optional: shows current trigger %
    }

    public TierDisplay[] tierDisplays;

    [Header("Win Celebration")]
    public GameObject winPanel;
    public TMP_Text   winTierText;
    public TMP_Text   winAmountText;

    private void OnEnable()
    {
        ProgressiveJackpot.OnPoolsUpdated += Refresh;
        ProgressiveJackpot.OnJackpotWon   += ShowWin;
    }

    private void OnDisable()
    {
        ProgressiveJackpot.OnPoolsUpdated -= Refresh;
        ProgressiveJackpot.OnJackpotWon   -= ShowWin;
    }

    private void Start() => Refresh();

    private void Refresh()
    {
        if (ProgressiveJackpot.Instance == null) return;
        foreach (var d in tierDisplays)
        {
            if (d.poolText   != null)
                d.poolText.text   = $"{d.tier}  {FormatCoins(ProgressiveJackpot.Instance.GetPool(d.tier))}";
            if (d.chanceText != null)
            {
                float pct = ProgressiveJackpot.Instance.GetChance(d.tier) * 100f;
                d.chanceText.text = $"{pct:F3}%";
            }
        }
    }

    private void ShowWin(ProgressiveJackpot.JackpotTier tier, long amount, string label)
    {
        winPanel?.SetActive(true);
        if (winTierText   != null) winTierText.text   = $"🎉 {label} JACKPOT!";
        if (winAmountText != null) winAmountText.text = $"+{FormatCoins(amount)}";
        Invoke(nameof(HideWin), 5f);
    }

    private void HideWin() => winPanel?.SetActive(false);

    private static string FormatCoins(long v)
    {
        if (v >= 1_000_000_000_000L) return $"{v / 1_000_000_000_000L}T";
        if (v >= 1_000_000_000L)     return $"{v / 1_000_000_000L}B";
        if (v >= 1_000_000L)         return $"{v / 1_000_000L}M";
        if (v >= 1_000L)             return $"{v / 1_000L}K";
        return v.ToString();
    }
}
