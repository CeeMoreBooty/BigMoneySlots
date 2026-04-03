using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings panel controller.
///
/// Manages:
///   • Sound on/off
///   • Music on/off
///   • Notifications on/off
///   • Auto-spin speed (Normal / Fast / Max)
///   • Privacy: clear local data
///   • Version info display
///
/// All settings persist via PlayerPrefs.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Toggles")]
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle notifToggle;

    [Header("Auto Spin Speed")]
    public Slider autoSpinSpeedSlider;   // 0=Normal(1s), 1=Fast(0.5s), 2=Max(0.1s)
    public TMP_Text autoSpinSpeedLabel;

    [Header("Misc")]
    public TMP_Text versionText;
    public Button   clearDataButton;
    public Button   closeButton;

    // ── PlayerPrefs keys ──────────────────────────────────────────────────────
    private const string KeySound  = "settings_sound";
    private const string KeyMusic  = "settings_music";
    private const string KeyNotif  = "settings_notif";
    private const string KeySpeed  = "settings_autospin_speed";

    private static readonly float[] SpeedValues  = { 1.0f, 0.5f, 0.1f };
    private static readonly string[] SpeedLabels = { "Normal (1 s)", "Fast (0.5 s)", "Max (0.1 s)" };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load persisted values
        if (soundToggle  != null)
        {
            soundToggle.isOn = PlayerPrefs.GetInt(KeySound, 1) == 1;
            soundToggle.onValueChanged.AddListener(v => {
                AudioListener.volume = v ? 1f : 0f;
                PlayerPrefs.SetInt(KeySound, v ? 1 : 0);
                PlayerPrefs.Save();
            });
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = PlayerPrefs.GetInt(KeyMusic, 1) == 1;
            musicToggle.onValueChanged.AddListener(v => {
                PlayerPrefs.SetInt(KeyMusic, v ? 1 : 0);
                PlayerPrefs.Save();
                // TODO: mute/unmute background music AudioSource
            });
        }

        if (notifToggle != null)
        {
            notifToggle.isOn = PlayerPrefs.GetInt(KeyNotif, 1) == 1;
            notifToggle.onValueChanged.AddListener(v => {
                PlayerPrefs.SetInt(KeyNotif, v ? 1 : 0);
                PlayerPrefs.Save();
            });
        }

        if (autoSpinSpeedSlider != null)
        {
            autoSpinSpeedSlider.minValue    = 0;
            autoSpinSpeedSlider.maxValue    = 2;
            autoSpinSpeedSlider.wholeNumbers = true;
            autoSpinSpeedSlider.value       = PlayerPrefs.GetInt(KeySpeed, 0);
            autoSpinSpeedSlider.onValueChanged.AddListener(v => ApplyAutoSpinSpeed((int)v));
            ApplyAutoSpinSpeed((int)autoSpinSpeedSlider.value);
        }

        if (versionText != null)
            versionText.text = $"v{Application.version} | Unity {Application.unityVersion}";

        clearDataButton?.onClick.AddListener(OnClearData);
        closeButton?.onClick.AddListener(Hide);

        // Apply sound on start (in case user muted last session)
        AudioListener.volume = PlayerPrefs.GetInt(KeySound, 1) == 1 ? 1f : 0f;
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    public void Show() => panel?.SetActive(true);
    public void Hide() => panel?.SetActive(false);
    public void Toggle() { if (panel != null) panel.SetActive(!panel.activeSelf); }

    // ── Internal ──────────────────────────────────────────────────────────────
    private void ApplyAutoSpinSpeed(int index)
    {
        index = Mathf.Clamp(index, 0, 2);
        PlayerPrefs.SetInt(KeySpeed, index);
        PlayerPrefs.Save();

        float delay = SpeedValues[index];
        var sm = FindObjectOfType<SlotMachine>();
        if (sm != null) sm.autoSpinDelay = delay;

        if (autoSpinSpeedLabel != null)
            autoSpinSpeedLabel.text = $"Auto Speed: {SpeedLabels[index]}";
    }

    private void OnClearData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Settings] All local data cleared.");
        // Reload the current scene to reset all singletons.
        SceneLoader.Instance?.ReloadCurrent();
    }
}
