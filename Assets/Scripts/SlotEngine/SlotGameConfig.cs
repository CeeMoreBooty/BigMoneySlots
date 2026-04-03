using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSlotGame", menuName = "SlotApp/SlotGameConfig")]
public class SlotGameConfig : ScriptableObject
{
    [Header("Identity")]
    public string gameId;
    public string gameName;
    public string theme;
    [TextArea(2, 4)]
    public string description;

    [Header("Reel Layout")]
    public int reelCount   = 5;
    public int rowCount    = 3;
    public int paylineCount = 20;

    [Header("Economy")]
    public long minBet    = 100;
    public long maxBet    = 10_000_000;
    [Range(0.8f, 1f)]
    public float baseRTP  = 0.92f;   // Return-to-player
    [Range(1f, 500f)]
    public float maxWinMultiplier = 100f;

    [Header("Volatility")]
    public Volatility volatility = Volatility.Medium;

    [Header("Features")]
    public bool hasWildSymbol;
    public bool hasScatterSymbol;
    public bool hasBonusRound;
    public bool hasFreeSpins;
    public bool hasProgressiveJackpot;
    public int  freeSpinsCount = 10;

    [Header("Symbols")]
    public string[] symbolNames;   // Designer assigns matching Symbol assets in Unity

    [Header("Visuals")]
    public Color  backgroundColor = Color.black;
    public Color  accentColor     = Color.yellow;
    public string iconSpriteName;  // Name of icon sprite in Resources folder

    public enum Volatility { Low, Medium, High, VeryHigh }
}
