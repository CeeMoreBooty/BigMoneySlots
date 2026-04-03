using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPayoutTable", menuName = "SlotApp/PayoutTable")]
public class PayoutTable : ScriptableObject
{
    [Serializable]
    public class PayoutEntry
    {
        public int symbolId;
        public float multiplier2OfAKind;
        public float multiplier3OfAKind;
    }

    public PayoutEntry[] entries;

    public float GetMultiplier(int symbolId, int matchCount)
    {
        foreach (var entry in entries)
        {
            if (entry.symbolId != symbolId) continue;
            return matchCount >= 3 ? entry.multiplier3OfAKind :
                   matchCount == 2 ? entry.multiplier2OfAKind : 0f;
        }
        return 0f;
    }
}
