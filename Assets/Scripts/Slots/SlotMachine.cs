using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core slot machine logic: spin orchestration, payline evaluation, jackpot.
/// </summary>
public class SlotsSlotMachine : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SlotConfig config;

    [Header("Reels")]
    [SerializeField] private ReelController[] reels;

    [Header("Spin Gaps (seconds between each reel stop)")]
    [SerializeField] private float reelStopGap = 0.25f;

    // Events
    public event Action<long>    OnWin;        // total win amount
    public event Action<long>    OnJackpot;    // jackpot win amount
    public event Action          OnFreeSpins;  // free spins triggered
    public event Action          OnSpinStart;
    public event Action          OnSpinEnd;

    private bool isSpinning;
    private int  freeSpinsRemaining;
    private float freeSpinsMultiplier;

    public bool  IsSpinning        => isSpinning;
    public bool  HasFreeSpins      => freeSpinsRemaining > 0;
    public int   FreeSpinsLeft     => freeSpinsRemaining;

    private long currentJackpot;

    private void Start()
    {
        currentJackpot = config != null ? config.baseJackpot : 1_000_000L;

        if (config != null)
            foreach (var reel in reels)
                reel?.Initialize(config);
    }

    /// <summary>Trigger a spin. Returns false if already spinning or insufficient funds.</summary>
    public bool TrySpin(long betAmount)
    {
        if (isSpinning) return false;

        bool isFree = HasFreeSpins;
        if (!isFree && !GameData.SpendCoins(betAmount)) return false;

        StartCoroutine(SpinRoutine(betAmount, isFree));
        return true;
    }

    private IEnumerator SpinRoutine(long betAmount, bool isFreeSpinRound)
    {
        isSpinning = true;
        OnSpinStart?.Invoke();
        SoundManager.Instance?.PlaySpin();

        if (isFreeSpinRound) freeSpinsRemaining--;

        // Generate results for all reels
        int[][] results = new int[reels.Length][];
        for (int r = 0; r < reels.Length; r++)
            results[r] = GenerateReelResult();

        // Stagger reel spins
        var spinCoroutines = new Coroutine[reels.Length];
        for (int r = 0; r < reels.Length; r++)
        {
            float delay = r * reelStopGap;
            spinCoroutines[r] = StartCoroutine(reels[r].SpinAndStop(results[r], delay));
        }

        // Wait for all reels
        foreach (var co in spinCoroutines)
            yield return co;

        // Evaluate paylines
        float multiplier = isFreeSpinRound ? freeSpinsMultiplier : 1f;
        long totalWin = EvaluatePaylines(results, betAmount, multiplier);

        // Check jackpot (all reels show jackpot symbol on middle row)
        bool jackpotHit = IsJackpotHit(results);
        if (jackpotHit)
        {
            long jackpotWin = currentJackpot;
            GameData.AddCoins(jackpotWin);
            OnJackpot?.Invoke(jackpotWin);
            SoundManager.Instance?.PlayJackpot();
            currentJackpot = config.baseJackpot; // reset
        }

        // Check scatter / free spins
        int scatterCount = CountScatters(results);
        if (scatterCount >= (config?.scatterCountForFreeSpins ?? 3))
        {
            freeSpinsRemaining += config?.freeSpinsAwarded ?? 10;
            freeSpinsMultiplier = config?.freeSpinsMultiplier ?? 2f;
            OnFreeSpins?.Invoke();
            SoundManager.Instance?.PlayBonus();
        }

        if (totalWin > 0)
        {
            GameData.AddCoins(totalWin);
            GameData.TotalWins++;
            GameData.BiggestWin = totalWin;
            OnWin?.Invoke(totalWin);

            if (totalWin >= betAmount * 10)
                SoundManager.Instance?.PlayBigWin();
            else
                SoundManager.Instance?.PlayWin();
        }

        GameData.TotalSpins++;
        currentJackpot += betAmount / 100; // small jackpot seed contribution
        GameData.Save();

        isSpinning = false;
        OnSpinEnd?.Invoke();
    }

    private int[] GenerateReelResult()
    {
        int rows = config?.numRows ?? 3;
        int[] result = new int[rows];
        for (int i = 0; i < rows; i++)
            result[i] = WeightedRandom();
        return result;
    }

    private int WeightedRandom()
    {
        if (config?.symbols == null || config.symbols.Length == 0) return 0;

        int totalWeight = 0;
        foreach (var sym in config.symbols)
            totalWeight += sym.weight;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < config.symbols.Length; i++)
        {
            roll -= config.symbols[i].weight;
            if (roll < 0) return i;
        }
        return config.symbols.Length - 1;
    }

    private long EvaluatePaylines(int[][] results, long bet, float multiplier)
    {
        if (config?.paylines == null) return 0;

        long total = 0;
        foreach (var payline in config.paylines)
        {
            long win = EvaluatePayline(results, payline, bet);
            total += (long)(win * multiplier);
        }
        return total;
    }

    private long EvaluatePayline(int[][] results, PaylineDefinition payline, long bet)
    {
        if (payline.rowIndices == null || payline.rowIndices.Length == 0) return 0;

        int firstSymbol = results[0][payline.rowIndices[0]];
        if (config.symbols[firstSymbol].isScatter || config.symbols[firstSymbol].isBonus) return 0;

        int matchCount = 1;
        for (int r = 1; r < reels.Length && r < payline.rowIndices.Length; r++)
        {
            int sym = results[r][payline.rowIndices[r]];
            bool match = sym == firstSymbol || config.symbols[sym].isWild || config.symbols[firstSymbol].isWild;
            if (match) matchCount++;
            else break;
        }

        if (matchCount < 3) return 0;

        var symbolDef = config.symbols[firstSymbol];
        long payout = matchCount switch
        {
            3 => symbolDef.payout3,
            4 => symbolDef.payout4,
            5 => symbolDef.payout5,
            _ => 0
        };
        return bet * payout;
    }

    private bool IsJackpotHit(int[][] results)
    {
        if (config?.symbols == null) return false;
        int middleRow = (config.numRows - 1) / 2;
        for (int r = 0; r < reels.Length; r++)
        {
            if (!config.symbols[results[r][middleRow]].isJackpot)
                return false;
        }
        return true;
    }

    private int CountScatters(int[][] results)
    {
        if (config?.symbols == null) return 0;
        int count = 0;
        foreach (var reelResult in results)
            foreach (var symIdx in reelResult)
                if (config.symbols[symIdx].isScatter) count++;
        return count;
    }

    public long CurrentJackpot => currentJackpot;
}
