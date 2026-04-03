using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs in Bootstrap (scene 0): initialises any cross-scene singletons then
/// loads the next scene.  Defaults to scene index 1 (MainMenu).
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    [Tooltip("Build index of the scene to load after Bootstrap.")]
    public int nextSceneIndex = 1;

    private IEnumerator Start()
    {
        // Give all Awake() calls in Bootstrap a chance to finish.
        yield return null;

        // Ensure DeviceTracker and Analytics are initialised before any scene.
        if (DeviceTracker.Instance   == null) gameObject.AddComponent<DeviceTracker>();
        if (AnalyticsManager.Instance == null) gameObject.AddComponent<AnalyticsManager>();

        Debug.Log($"[Bootstrap] Loading scene {nextSceneIndex}...");
        SceneManager.LoadScene(nextSceneIndex);
    }
}
