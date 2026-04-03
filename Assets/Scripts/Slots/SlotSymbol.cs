using System;
using UnityEngine;

/// <summary>
/// Defines a slot symbol's identity, sprite, and payout multipliers.
/// </summary>
[Serializable]
public class SlotSymbol
{
    public string symbolName;
    public Sprite sprite;
    [Range(0, 100)] public int weight = 10; // probability weight

    [Header("Payouts (multiplier × bet)")]
    public int payout3 = 5;
    public int payout4 = 15;
    public int payout5 = 50;

    public bool isWild      = false;
    public bool isScatter   = false;
    public bool isBonus     = false;
    public bool isJackpot   = false;
}
