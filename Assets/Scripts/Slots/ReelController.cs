using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a single slot reel: spin animation, stop on target symbols.
/// </summary>
public class ReelController : MonoBehaviour
{
    [Header("Reel Settings")]
    [SerializeField] private float spinSpeed        = 1800f; // degrees per second equiv (px/s)
    [SerializeField] private float symbolHeight     = 200f;
    [SerializeField] private int   visibleRows      = 3;
    [SerializeField] private float stopDecelerationTime = 0.4f;

    [Header("References")]
    [SerializeField] private RectTransform symbolContainer;
    [SerializeField] private Image[]       symbolImages;  // visible rows top→bottom

    private SlotConfig config;
    private int[] currentSymbolIndices;
    private bool spinning;

    public bool IsSpinning => spinning;

    public void Initialize(SlotConfig cfg)
    {
        config = cfg;
        currentSymbolIndices = new int[visibleRows];
        RandomizeSymbols();
        UpdateVisuals();
    }

    public void RandomizeSymbols()
    {
        for (int i = 0; i < visibleRows; i++)
            currentSymbolIndices[i] = WeightedRandom();
    }

    /// <summary>Spin and then stop at the provided target row indices.</summary>
    public IEnumerator SpinAndStop(int[] targetIndices, float delay)
    {
        spinning = true;
        yield return StartCoroutine(SpinLoop());
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(SlowAndStop(targetIndices));
        spinning = false;
    }

    private IEnumerator SpinLoop()
    {
        // Animate symbols scrolling downward for a minimum duration
        float minSpinTime = 0.8f;
        float elapsed = 0f;
        float offset = 0f;

        while (elapsed < minSpinTime)
        {
            elapsed += Time.deltaTime;
            offset += spinSpeed * Time.deltaTime;

            if (offset >= symbolHeight)
            {
                offset -= symbolHeight;
                // Shift symbols up (new one appears at top)
                ShiftSymbolsDown();
            }

            if (symbolContainer != null)
                symbolContainer.anchoredPosition = new Vector2(0, -offset);

            yield return null;
        }

        // Reset container position
        if (symbolContainer != null)
            symbolContainer.anchoredPosition = Vector2.zero;
    }

    private IEnumerator SlowAndStop(int[] targetIndices)
    {
        // Snap to target
        currentSymbolIndices = (int[])targetIndices.Clone();
        UpdateVisuals();

        // Small bounce effect
        float elapsed = 0f;
        float bounceHeight = 15f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float y = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            if (symbolContainer != null)
                symbolContainer.anchoredPosition = new Vector2(0, -y);
            yield return null;
        }

        if (symbolContainer != null)
            symbolContainer.anchoredPosition = Vector2.zero;

        SoundManager.Instance?.PlayReelStop();
    }

    private void ShiftSymbolsDown()
    {
        for (int i = currentSymbolIndices.Length - 1; i > 0; i--)
            currentSymbolIndices[i] = currentSymbolIndices[i - 1];
        currentSymbolIndices[0] = WeightedRandom();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (config == null || symbolImages == null) return;

        for (int i = 0; i < symbolImages.Length && i < currentSymbolIndices.Length; i++)
        {
            int idx = currentSymbolIndices[i];
            if (idx >= 0 && idx < config.symbols.Length && symbolImages[i] != null)
                symbolImages[i].sprite = config.symbols[idx].sprite;
        }
    }

    private int WeightedRandom()
    {
        if (config == null || config.symbols == null || config.symbols.Length == 0) return 0;

        int totalWeight = 0;
        foreach (var sym in config.symbols)
            totalWeight += sym.weight;

        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < config.symbols.Length; i++)
        {
            roll -= config.symbols[i].weight;
            if (roll < 0) return i;
        }
        return config.symbols.Length - 1;
    }

    /// <summary>Returns the symbol index showing at the given row.</summary>
    public int GetSymbolAt(int row)
    {
        if (row < 0 || row >= currentSymbolIndices.Length) return 0;
        return currentSymbolIndices[row];
    }

    public void HighlightSymbol(int row, bool highlight)
    {
        if (symbolImages == null || row >= symbolImages.Length) return;
        symbolImages[row].color = highlight ? Color.yellow : Color.white;
    }

    public void ClearHighlights()
    {
        if (symbolImages == null) return;
        foreach (var img in symbolImages)
            if (img != null) img.color = Color.white;
    }
}
