using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs in Bootstrap (scene 0): shows Terms and Conditions on first launch,
/// initialises cross-scene singletons, then loads MainMenu.
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    [Tooltip("Build index of the scene to load after Bootstrap.")]
    public int nextSceneIndex = 1;

    private IEnumerator Start()
    {
        // Give all Awake() calls in Bootstrap a chance to finish.
        yield return null;

        // Show Terms & Conditions if not yet accepted (blocks until player taps Accept).
        if (!PlayerPrefs.HasKey("tc_accepted") || PlayerPrefs.GetInt("tc_accepted", 0) < 1)
        {
            gameObject.AddComponent<TermsAndConditionsUI>();
            // Wait until the player accepts or a 5-minute timeout elapses
            float waited = 0f;
            const float timeout = 300f;
            while (PlayerPrefs.GetInt("tc_accepted", 0) < 1 && waited < timeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            // If timed out without acceptance, quit gracefully
            if (PlayerPrefs.GetInt("tc_accepted", 0) < 1)
            {
                Debug.LogWarning("[Bootstrap] T&C not accepted within timeout — quitting.");
                Application.Quit();
                yield break;
            }
        }

        // Ensure DeviceTracker and Analytics are initialised before any scene.
        if (DeviceTracker.Instance    == null) gameObject.AddComponent<DeviceTracker>();
        if (AnalyticsManager.Instance == null) gameObject.AddComponent<AnalyticsManager>();

        Debug.Log($"[Bootstrap] Loading scene {nextSceneIndex}...");
        SceneManager.LoadScene(nextSceneIndex);
    }
}
