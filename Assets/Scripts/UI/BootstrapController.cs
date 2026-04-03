using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Entry-point scene controller. Initialises persistent singletons then loads
/// the Main Menu. Attach to the single GameObject in Bootstrap.unity.
/// </summary>
public class BootstrapController : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private Slider  progressBar;
    [SerializeField] private float   minLoadTime = 1.5f;

    private IEnumerator Start()
    {
        // Allow the first frame to render the splash/loading UI
        yield return null;

        float startTime = Time.realtimeSinceStartup;
        float progress  = 0f;

        // ── Step 1: Ensure UIManager singleton exists ──────────────────────
        if (UIManager.Instance == null)
        {
            var go = new GameObject("UIManager");
            go.AddComponent<UIManager>();
        }
        progress = 0.25f;
        SetProgress(progress);
        yield return null;

        // ── Step 2: Ensure SoundManager singleton exists ───────────────────
        if (SoundManager.Instance == null)
        {
            var go = new GameObject("SoundManager");
            go.AddComponent<AudioSource>(); // music
            go.AddComponent<AudioSource>(); // sfx
            go.AddComponent<SoundManager>();
        }
        progress = 0.5f;
        SetProgress(progress);
        yield return null;

        // ── Step 3: First-run setup ────────────────────────────────────────
        if (!PlayerPrefs.HasKey("FirstRun"))
        {
            PlayerPrefs.SetInt("FirstRun", 1);
            GameData.Coins  = 10_000L;
            GameData.Level  = 1;
            GameData.Save();
        }
        progress = 0.8f;
        SetProgress(progress);
        yield return null;

        // ── Step 4: Ensure minimum display time for a smooth experience ────
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minLoadTime)
            yield return new WaitForSeconds(minLoadTime - elapsed);

        progress = 1f;
        SetProgress(progress);
        yield return null;

        // ── Step 5: Load Main Menu ─────────────────────────────────────────
        UIManager.Instance?.GoToMainMenu();
    }

    private void SetProgress(float value)
    {
        if (progressBar != null) progressBar.value = Mathf.Clamp01(value);
    }
}
