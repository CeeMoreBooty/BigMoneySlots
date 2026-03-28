using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core slot machine logic: reel spinning, win evaluation, and bet management.
///
/// Economy: uses <see cref="PlayerEconomy"/> singleton when available,
/// falling back to <see cref="GameManager.userData"/> for legacy support.
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
    private long _currentBet                = 1_000L;

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

    private Symbol[,] _reelResult;
    private bool _isSpinning;

    /// <summary>Fires with full reel grid data and win level (used by SlotMachineUI / SlotGameSceneController).</summary>
    public event Action<Symbol[,], long, WinLevel> OnSpinFinished;

    /// <summary>Fires with simple (payout, isJackpot) payload (used by SlotUI).</summary>
    public event Action<long, bool> OnSpinComplete;

    public long CurrentBet => _currentBet;

    /// <summary>Field alias so Inspector-serialised <c>betAmount</c> values are honoured.</summary>
    public long betAmount
    {
        get => _currentBet;
        set => SetBet(value);
    }

    public void SetBet(long bet)
    {
        _currentBet = bet < minBet ? minBet : (bet > maxBet ? maxBet : bet);
    }

    public void IncreaseBet()
    {
        long[] steps = { 1_000, 5_000, 10_000, 25_000, 50_000,
                         100_000, 500_000, 1_000_000, 5_000_000,
                         10_000_000, 50_000_000, 100_000_000 };
        for (int i = 0; i < steps.Length - 1; i++)
        {
            if (_currentBet < steps[i + 1]) { _currentBet = steps[i + 1]; return; }
        }
        _currentBet = maxBet;
    }

    public void DecreaseBet()
    {
        long[] steps = { 1_000, 5_000, 10_000, 25_000, 50_000,
                         100_000, 500_000, 1_000_000, 5_000_000,
                         10_000_000, 50_000_000, 100_000_000 };
        for (int i = steps.Length - 1; i > 0; i--)
        {
            if (_currentBet > steps[i - 1]) { _currentBet = steps[i - 1]; return; }
        }
        _currentBet = minBet;
    }

    public void Spin()
    {
        if (_isSpinning) return;

        // Deduct bet — prefer the PlayerEconomy singleton, fall back to UserData via GameManager
        bool spent = false;
        if (PlayerEconomy.Instance != null)
            spent = PlayerEconomy.Instance.SpendCoins(_currentBet);
        else if (GameManager.Instance?.userData != null)
            spent = GameManager.Instance.userData.SpendCoins(_currentBet);

        if (!spent) return;

        _isSpinning = true;
        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        _reelResult = GenerateResult();
        long payout = CalculatePayout(_reelResult, _currentBet, out WinLevel winLevel);

        // Apply multipliers and award payout
        if (payout > 0)
        {
            float mult = PlayerEconomy.Instance?.GetActiveMultiplier() ?? 1f;
            payout = (long)(payout * mult);
            if (LoyaltySystem.Instance != null)
                payout = LoyaltySystem.Instance.ApplyTierBonus(payout);

            if (PlayerEconomy.Instance != null)
                PlayerEconomy.Instance.AddCoins(payout);
            else
                GameManager.Instance?.userData?.AddCoins(payout);
        }

        LoyaltySystem.Instance?.RegisterSpin();
        _isSpinning = false;

        OnSpinFinished?.Invoke(_reelResult, payout, winLevel);
        OnSpinComplete?.Invoke(payout, false);
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

        // Check middle payline (row index 1)
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
            // Partial match: count consecutive symbols from left
            int matchCount = 1;
            Symbol s0 = result[0, 1];
            for (int r = 1; r < reelCount; r++)
            {
                Symbol s = result[r, 1];
                if (s == s0 || s == Symbol.Wild) matchCount++;
                else break;
            }
            if (matchCount >= 3)
                totalPayout = bet * (matchCount - 2) * 2;
        }

        float ratio = bet > 0 ? (float)totalPayout / bet : 0f;
        if      (ratio >= 100f) winLevel = WinLevel.EpicWin;
        else if (ratio >= 25f)  winLevel = WinLevel.MegaWin;
        else if (ratio >= 10f)  winLevel = WinLevel.BigWin;
        else if (ratio > 0f)    winLevel = WinLevel.Normal;

        return totalPayout;
    }
}
