using UnityEngine;

public class ProgressiveJackpot : MonoBehaviour
{
    public static ProgressiveJackpot Instance { get; private set; }

    [SerializeField] private long baseJackpot = 1_000_000;
    [SerializeField] private float contributionRate = 0.01f; // 1% of each bet

    public long CurrentJackpot { get; private set; }

    private const string KeyJackpot = "jackpot_current";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentJackpot = long.TryParse(PlayerPrefs.GetString(KeyJackpot, "0"), out long saved) && saved > baseJackpot
            ? saved : baseJackpot;
    }

    public void Contribute(long betAmount)
    {
        CurrentJackpot += (long)(betAmount * contributionRate);
        Save();
    }

    public long Payout()
    {
        long amount = CurrentJackpot;
        Reset();
        return amount;
    }

    private void Reset()
    {
        CurrentJackpot = baseJackpot;
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetString(KeyJackpot, CurrentJackpot.ToString());
        PlayerPrefs.Save();
    }
}
