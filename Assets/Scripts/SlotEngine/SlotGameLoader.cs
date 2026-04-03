using System;
using UnityEngine;

/// <summary>
/// Loads and activates a slot game from the registry.
/// Broadcast SlotGameLoader.OnGameChanged whenever the active game switches.
/// </summary>
public class SlotGameLoader : MonoBehaviour
{
    public static SlotGameLoader Instance { get; private set; }

    [Header("Registry")]
    public SlotGameRegistry registry;

    public SlotGameConfig ActiveGame { get; private set; }

    public static event Action<SlotGameConfig> OnGameChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load the last played game, or default to first in registry
        string savedId = PlayerPrefs.GetString("active_game_id", "");
        var game = !string.IsNullOrEmpty(savedId) ? registry?.GetById(savedId) : null;
        if (game == null && registry != null && registry.games != null && registry.games.Count > 0)
            game = registry.games[0];
        LoadGame(game);
    }

    /// <summary>Load a game by its gameId string.</summary>
    public void LoadGameById(string gameId)
    {
        var game = registry?.GetById(gameId);
        if (game == null) { Debug.LogWarning($"[SlotGameLoader] Game '{gameId}' not found."); return; }
        LoadGame(game);
    }

    /// <summary>Load a random game from the registry.</summary>
    public void LoadRandomGame()
    {
        var game = registry?.GetRandom();
        if (game != null) LoadGame(game);
    }

    /// <summary>Unload the current slot game and return to the lobby scene.</summary>
    public void ReturnToLobby()
    {
        ActiveGame = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    private void LoadGame(SlotGameConfig game)
    {
        if (game == null) return;
        ActiveGame = game;
        PlayerPrefs.SetString("active_game_id", game.gameId);
        PlayerPrefs.Save();
        OnGameChanged?.Invoke(game);
        Debug.Log($"[SlotGameLoader] Loaded game: {game.gameName}");
    }
}
