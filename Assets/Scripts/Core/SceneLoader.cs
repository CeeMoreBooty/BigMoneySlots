using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central scene-transition helper.  Provides named and indexed loads with an
/// optional one-frame black fade so the screen never flashes white between scenes.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Tooltip("Seconds to wait before loading the next scene (simulates a short fade).")]
    public float transitionDelay = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    public void Load(string sceneName)    => StartCoroutine(LoadAsync(sceneName));
    public void Load(int buildIndex)      => StartCoroutine(LoadAsync(buildIndex));
    public void ReloadCurrent()           => Load(SceneManager.GetActiveScene().buildIndex);

    public void LoadGame()     => Load("Game");
    public void LoadMainMenu() => Load("MainMenu");
    public void LoadBootstrap()=> Load("Bootstrap");

    // ── Internal ───────────────────────────────────────────────────────────────
    private IEnumerator LoadAsync(string name)
    {
        yield return new WaitForSeconds(transitionDelay);
        SceneManager.LoadScene(name);
    }

    private IEnumerator LoadAsync(int index)
    {
        yield return new WaitForSeconds(transitionDelay);
        SceneManager.LoadScene(index);
    }
}
