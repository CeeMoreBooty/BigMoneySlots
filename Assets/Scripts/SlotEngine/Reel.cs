using UnityEngine;

public class Reel : MonoBehaviour
{
    public Symbol[] availableSymbols;

    public Symbol CurrentSymbol { get; private set; }

    public Symbol Spin()
    {
        if (availableSymbols == null || availableSymbols.Length == 0)
        {
            Debug.LogWarning("Reel has no symbols assigned.");
            return null;
        }
        CurrentSymbol = availableSymbols[SecureRandom.Range(0, availableSymbols.Length)];
        return CurrentSymbol;
    }
}
