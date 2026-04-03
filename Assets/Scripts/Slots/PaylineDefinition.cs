using System;
using UnityEngine;

/// <summary>
/// Defines a single payline as a sequence of row indices per reel column.
/// </summary>
[Serializable]
public class PaylineDefinition
{
    public string paylineName;
    public int[] rowIndices; // length == number of reels (e.g., 5)
    public Color lineColor = Color.white;
}
