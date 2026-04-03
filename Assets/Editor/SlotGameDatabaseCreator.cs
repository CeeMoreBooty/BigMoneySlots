using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that generates one SlotGameConfig ScriptableObject per entry in
/// SlotGameDatabase.All and saves them under Assets/Resources/SlotGames/.
/// Run via: Tools → Slot Games → Generate All Configs
/// </summary>
public static class SlotGameDatabaseCreator
{
    private const string OutputFolder = "Assets/Resources/SlotGames";

    [MenuItem("Tools/Slot Games/Generate All Configs")]
    public static void GenerateAllConfigs()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "SlotGames");

        int created = 0;
        int updated = 0;

        foreach (var def in SlotGameDatabase.All)
        {
            string assetPath = $"{OutputFolder}/{def.GameId}.asset";
            bool isNew = false;

            var config = AssetDatabase.LoadAssetAtPath<SlotGameConfig>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SlotGameConfig>();
                isNew = true;
            }

            ApplyDefinition(config, def);

            if (isNew)
            {
                AssetDatabase.CreateAsset(config, assetPath);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(config);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SlotGameDatabaseCreator] Done — {created} created, {updated} updated.");
        EditorUtility.DisplayDialog(
            "Generate All Configs",
            $"Done!\n\nCreated: {created}\nUpdated: {updated}\n\nAssets saved to {OutputFolder}",
            "OK");
    }

    private static void ApplyDefinition(SlotGameConfig config, SlotGameDatabase.GameDefinition def)
    {
        config.gameId            = def.GameId;
        config.gameName          = def.GameName;
        config.theme             = def.Theme;
        config.description       = def.Description;
        config.reelCount         = def.ReelCount;
        config.rowCount          = def.RowCount;
        config.paylineCount      = def.PaylineCount;
        config.minBet            = def.MinBet;
        config.maxBet            = def.MaxBet;
        config.baseRTP           = def.BaseRTP;
        config.maxWinMultiplier  = def.MaxWinMultiplier;
        config.volatility        = def.Volatility;
        config.hasWildSymbol     = def.HasWild;
        config.hasScatterSymbol  = def.HasScatter;
        config.hasBonusRound     = def.HasBonus;
        config.hasFreeSpins      = def.HasFreeSpins;
        config.hasProgressiveJackpot = def.HasProgressive;
        config.freeSpinsCount    = def.FreeSpinsCount;
        config.symbolNames       = def.SymbolNames;
        config.backgroundColor   = def.ToBackgroundColor();   // hex → Color
        config.accentColor       = def.ToAccentColor();        // hex → Color
    }
}
