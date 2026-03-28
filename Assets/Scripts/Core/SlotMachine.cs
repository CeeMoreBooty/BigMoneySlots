using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core slot machine logic: reel spinning, win evaluation, and bet management.
/// </summary>
public class SlotMachine : MonoBehaviour
{
    public enum Symbol
    {
        Cherry  = 0,
        Lemon   = 1,
        Orange  = 2,
        Plum    = 3,
        Bell    = 4,
        Bar     = 5,
        Seven   = 6,
        Diamond = 7,
        Wild    = 8
    }

    public enum WinLevel
    {
        None    = 0,
        Normal  = 1,
        BigWin  = 2,
        MegaWin = 3,
        EpicWin = 4
    }

    [Header("Slot Configuration")]
    [SerializeField] private int reelCount  = 5;
    [SerializeField] private int rowCount   = 3;

    [Header("Bet")]
    [SerializeField] private long minBet    = 1_000L;
    [SerializeField] private long maxBet    = 100_000_000L;
    private long currentBet                 = 1_000L;

    // Payline multipliers indexed by symbol
    private static readonly Dictionary<Symbol, long> SymbolMultiplier = new Dictionary<Symbol, long>
    {
        { Symbol.Cherry,  2   },
        { Symbol.Lemon,   3   },
        { Symbol.Orange,  5   },
        { Symbol.Plum,    8   },
        { Symbol.Bell,    15  },
        { Symbol.Bar,     25  },
        { Symbol.Seven,   50  },
        { Symbol.Diamond, 100 },
        { Symbol.Wild,    200 }
    };

    private Symbol[,] reelResult;
    private bool isSpinning;

    public event Action<Symbol[,], long, WinLevel> OnSpinFinished;

    public long CurrentBet => currentBet;

    public void SetBet(long bet)
    {
        currentBet = Mathf.Clamp((long)bet, minBet, maxBet);
    }

    public void IncreaseBet()
    {
        long[] steps = { 1_000, 5_000, 10_000, 25_000, 50_000,
                         100_000, 500_000, 1_000_000, 5_000_000,
                         10_000_000, 50_000_000, 100_000_000 };
        for (int i = 0; i < steps.Length - 1; i++)
        {
            if (currentBet < steps[i + 1])
            {
                currentBet = steps[i + 1];
                return;
            }
        }
        currentBet = maxBet;
    }

    public void DecreaseBet()
    {
        long[] steps = { 1_000, 5_000, 10_000, 25_000, 50_000,
                         100_000, 500_000, 1_000_000, 5_000_000,
                         10_000_000, 50_000_000, 100_000_000 };
        for (int i = steps.Length - 1; i > 0; i--)
        {
            if (currentBet > steps[i - 1])
            {
                currentBet = steps[i - 1];
                return;
            }
        }
        currentBet = minBet;
    }

    public void Spin()
    {
        if (isSpinning) return;
        if (!GameManager.Instance.userData.SpendCoins(currentBet)) return;

        isSpinning = true;
        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        yield return new WaitForSeconds(1.5f); // animation time

        reelResult = GenerateResult();
        long payout   = CalculatePayout(reelResult, currentBet, out WinLevel winLevel);

        isSpinning = false;
        OnSpinFinished?.Invoke(reelResult, payout, winLevel);
        GameManager.Instance?.OnSpinComplete(payout, winLevel);
    }

    private Symbol[,] GenerateResult()
    {
        var result = new Symbol[reelCount, rowCount];
        int symbolCount = Enum.GetValues(typeof(Symbol)).Length;
        for (int r = 0; r < reelCount; r++)
            for (int row = 0; row < rowCount; row++)
                result[r, row] = (Symbol)UnityEngine.Random.Range(0, symbolCount);
        return result;
    }

    private long CalculatePayout(Symbol[,] result, long bet, out WinLevel winLevel)
    {
        long totalPayout = 0L;
        winLevel = WinLevel.None;

        // Check middle payline
        bool allMatch = true;
        Symbol first = result[0, 1];
        for (int r = 1; r < reelCount; r++)
        {
            Symbol s = result[r, 1];
            if (s != first && s != Symbol.Wild && first != Symbol.Wild)
            {
                allMatch = false;
                break;
            }
        }

        if (allMatch)
        {
            Symbol paySymbol = (first == Symbol.Wild) ? result[1, 1] : first;
            if (SymbolMultiplier.TryGetValue(paySymbol, out long mult))
                totalPayout = bet * mult;
            else
                totalPayout = bet * 2;
        }
        else
        {
            // Count matching symbols from left
            int matchCount = 1;
            Symbol s0 = result[0, 1];
            for (int r = 1; r < reelCount; r++)
            {
                Symbol s = result[r, 1];
                if (s == s0 || s == Symbol.Wild)
                    matchCount++;
                else
                    break;
            }
            if (matchCount >= 3)
                totalPayout = bet * (matchCount - 2) * 2;
        }

        // Determine win level based on payout vs bet ratio
        float ratio = bet > 0 ? (float)totalPayout / bet : 0;
        if      (ratio >= 100) winLevel = WinLevel.EpicWin;
        else if (ratio >= 25)  winLevel = WinLevel.MegaWin;
        else if (ratio >= 10)  winLevel = WinLevel.BigWin;
        else if (ratio > 0)    winLevel = WinLevel.Normal;

        return totalPayout;
    }
}
