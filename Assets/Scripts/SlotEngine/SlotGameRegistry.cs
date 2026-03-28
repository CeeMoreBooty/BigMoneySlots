using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject registry holding references to all SlotGameConfig assets.
/// Assign all 40 game configs here in the Unity Inspector.
/// </summary>
[CreateAssetMenu(fileName = "SlotGameRegistry", menuName = "SlotApp/SlotGameRegistry")]
public class SlotGameRegistry : ScriptableObject
{
    public List<SlotGameConfig> games = new List<SlotGameConfig>();

    public SlotGameConfig GetById(string gameId)
    {
        foreach (var g in games)
            if (g.gameId == gameId) return g;
        return null;
    }

    public SlotGameConfig GetRandom()
    {
        if (games == null || games.Count == 0) return null;
        return games[Random.Range(0, games.Count)];
    }
}
