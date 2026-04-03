using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central navigation controller. Handles scene transitions with fade.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_GAME      = "Game";
    public const string SCENE_SHOP      = "Shop";
    public const string SCENE_SETTINGS  = "Settings";
    public const string SCENE_PAYTABLE  = "Paytable";

    [SerializeField] private Animator fadeAnimator;
    private static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");
    private static readonly int FadeInTrigger  = Animator.StringToHash("FadeIn");

    private string pendingScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public navigation methods ────────────────────────────────────────────

    public void GoToMainMenu()  => LoadScene(SCENE_MAIN_MENU);
    public void GoToGame()      => LoadScene(SCENE_GAME);
    public void GoToShop()      => LoadScene(SCENE_SHOP);
    public void GoToSettings()  => LoadScene(SCENE_SETTINGS);
    public void GoToPaytable()  => LoadScene(SCENE_PAYTABLE);

    public void LoadScene(string sceneName)
    {
        SoundManager.Instance?.PlayButtonClick();
        if (fadeAnimator != null)
        {
            pendingScene = sceneName;
            fadeAnimator.SetTrigger(FadeOutTrigger);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // Called by the fade-out animation event
    public void OnFadeOutComplete()
    {
        SceneManager.LoadScene(pendingScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
