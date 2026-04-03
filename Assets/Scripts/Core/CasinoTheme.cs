using UnityEngine;

/// <summary>
/// Centralised Vegas-style colour / sizing theme.
/// Assign one instance to CasinoHUD and LobbyController via the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "CasinoTheme", menuName = "SlotApp/CasinoTheme")]
public class CasinoTheme : ScriptableObject
{
    [Header("Background Colors")]
    public Color screenBackground    = new Color(0.04f, 0.04f, 0.10f);   // deep navy
    public Color panelBackground     = new Color(0.08f, 0.06f, 0.18f);   // dark purple
    public Color cardBackground      = new Color(0.10f, 0.08f, 0.22f);   // medium purple
    public Color cardBackgroundAlt   = new Color(0.06f, 0.04f, 0.16f);   // darker variant
    public Color hudBackground       = new Color(0.05f, 0.04f, 0.12f, 0.95f);
    public Color navBarBackground    = new Color(0.06f, 0.05f, 0.14f, 0.97f);

    [Header("Accent Colors")]
    public Color goldPrimary         = new Color(1.00f, 0.84f, 0.00f);   // #FFD700
    public Color goldSecondary       = new Color(1.00f, 0.70f, 0.10f);   // #FFB31A
    public Color goldDark            = new Color(0.72f, 0.50f, 0.00f);   // darker gold
    public Color platinumSilver      = new Color(0.85f, 0.87f, 0.90f);
    public Color neonPink            = new Color(1.00f, 0.08f, 0.58f);   // #FF1494
    public Color neonCyan            = new Color(0.00f, 0.90f, 1.00f);   // #00E5FF
    public Color neonGreen           = new Color(0.22f, 1.00f, 0.08f);   // #38FF14
    public Color errorRed            = new Color(1.00f, 0.22f, 0.22f);

    [Header("Text Colors")]
    public Color textPrimary         = Color.white;
    public Color textSecondary       = new Color(0.80f, 0.78f, 0.88f);
    public Color textMuted           = new Color(0.55f, 0.52f, 0.65f);
    public Color textGold            = new Color(1.00f, 0.84f, 0.00f);
    public Color textOnDark          = Color.white;

    [Header("Button Colors")]
    public Color spinButtonColor     = new Color(1.00f, 0.50f, 0.00f);   // vivid orange
    public Color spinButtonHighlight = new Color(1.00f, 0.70f, 0.10f);
    public Color betButtonColor      = new Color(0.15f, 0.12f, 0.30f);
    public Color disabledButton      = new Color(0.30f, 0.28f, 0.36f);

    [Header("Win Celebration")]
    public Color bigWinGlow          = new Color(1.00f, 0.70f, 0.00f, 0.85f);
    public Color megaWinGlow         = new Color(1.00f, 0.20f, 0.80f, 0.90f);
    public Color epicWinGlow         = new Color(0.40f, 0.00f, 1.00f, 0.95f);
    public Color jackpotGlow         = new Color(1.00f, 0.84f, 0.00f, 1.00f);

    [Header("Card Badge Colors")]
    public Color badgeNew            = new Color(0.20f, 0.80f, 0.20f);   // green
    public Color badgeHot            = new Color(1.00f, 0.30f, 0.00f);   // orange-red
    public Color badgeJackpot        = new Color(1.00f, 0.84f, 0.00f);   // gold
    public Color badgeLocked         = new Color(0.30f, 0.30f, 0.30f);   // grey

    [Header("HUD Sizing")]
    public float hudHeight           = 100f;
    public float navBarHeight        = 90f;
    public float gameCardWidth       = 320f;
    public float gameCardHeight      = 240f;
    public float gameCardSpacing     = 16f;
    public float cardCornerRadius    = 18f;

    [Header("Font Sizes")]
    public int fontSizeTitle         = 48;
    public int fontSizeSubtitle      = 32;
    public int fontSizeBody          = 24;
    public int fontSizeSmall         = 18;
    public int fontSizeHUDCoins      = 34;
    public int fontSizeBadge         = 20;
    public int fontSizeCardName      = 26;
    public int fontSizeCardTheme     = 18;

    [Header("Animation")]
    public float cardHoverScale      = 1.06f;
    public float cardPressScale      = 0.94f;
    public float spinButtonPulse     = 1.08f;
    public float coinFlyDuration     = 0.8f;
    public float winTextBobAmplitude = 12f;
    public float winTextBobFrequency = 2.5f;
}
