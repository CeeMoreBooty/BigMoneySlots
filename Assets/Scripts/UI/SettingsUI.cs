using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings panel controller.
///
/// UIBuilder creates and assigns <see cref="panel"/> at runtime; no Inspector
/// wiring is needed.  All settings persist via PlayerPrefs.
///
/// Exposed API used by UIBuilder and LobbyUI:
///   Show() / Hide() / Toggle()
/// </summary>
public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    // ── Inspector-assignable (or set at runtime by UIBuilder) ─────────────────
    [Header("Panel root — assigned at runtime by UIBuilder if null")]
    public GameObject panel;

    [Header("Toggles — optional: UIBuilder builds its own if null")]
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle notifToggle;

    [Header("Misc — optional")]
    public TMP_Text versionText;
    public Button   clearDataButton;
    public Button   closeButton;

    // PlayerPrefs keys
    private const string KeySound = "settings_sound";
    private const string KeyMusic = "settings_music";
    private const string KeyNotif = "settings_notif";
    private const string KeySpeed = "settings_autospin_speed";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Apply persisted sound volume immediately on every scene load.
        AudioListener.volume = PlayerPrefs.GetInt(KeySound, 1) == 1 ? 1f : 0f;
    }

    private void Start()
    {
        // Wire Inspector-assigned toggles if present (non-UIBuilder scenes).
        WireToggle(soundToggle, KeySound, v => AudioListener.volume = v ? 1f : 0f);
        WireToggle(musicToggle, KeyMusic, null);
        WireToggle(notifToggle, KeyNotif, null);

        if (versionText != null)
            versionText.text = "v" + Application.version +
                               "  |  Unity " + Application.unityVersion;

        clearDataButton?.onClick.AddListener(ClearData);
        closeButton?.onClick.AddListener(Hide);

        // Hide panel on start (UIBuilder shows it on button press)
        if (panel != null) panel.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show()   { if (panel != null) panel.SetActive(true); }
    public void Hide()   { if (panel != null) panel.SetActive(false); }
    public void Toggle() { if (panel != null) panel.SetActive(!panel.activeSelf); }

    /// <summary>Apply an auto-spin delay to the active SlotMachine.</summary>
    public void SetAutoSpinSpeed(float delay)
    {
        var sm = FindObjectOfType<SlotMachine>();
        if (sm != null) sm.autoSpinDelay = delay;
        PlayerPrefs.SetFloat(KeySpeed, delay);
        PlayerPrefs.Save();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void WireToggle(Toggle toggle, string key, System.Action<bool> extra)
    {
        if (toggle == null) return;
        toggle.isOn = PlayerPrefs.GetInt(key, 1) == 1;
        toggle.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetInt(key, v ? 1 : 0);
            PlayerPrefs.Save();
            extra?.Invoke(v);
        });
    }

    private void ClearData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SettingsUI] All local data cleared — reloading scene.");
        SceneLoader.Instance?.ReloadCurrent();
    }
}
