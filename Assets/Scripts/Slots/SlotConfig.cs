using UnityEngine;

/// <summary>
/// ScriptableObject holding the full configuration for a slot machine.
/// </summary>
[CreateAssetMenu(fileName = "SlotConfig", menuName = "BigMoneySlots/Slot Config")]
public class SlotConfig : ScriptableObject
{
    [Header("Grid")]
    public int numReels  = 5;
    public int numRows   = 3;

    [Header("Betting")]
    public long minBet     = 100;
    public long maxBet     = 10000;
    public long betStep    = 100;
    public long defaultBet = 500;

    [Header("Jackpot")]
    public long baseJackpot = 1_000_000;

    [Header("Bonus")]
    public int scatterCountForFreeSpins = 3;
    public int freeSpinsAwarded         = 10;
    public float freeSpinsMultiplier    = 2f;

    [Header("Symbols")]
    public SlotSymbol[] symbols;

    [Header("Paylines")]
    public PaylineDefinition[] paylines;
}
