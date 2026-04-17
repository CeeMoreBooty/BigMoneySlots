using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings scene: music volume, SFX volume, notifications, reset.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Toggle Buttons")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle notificationsToggle;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private Button termsButton;

    [Header("Reset Confirmation")]
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button     confirmResetButton;
    [SerializeField] private Button     cancelResetButton;

    private void Start()
    {
        LoadSettings();

        musicSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicToggle?.onValueChanged.AddListener(OnMusicToggled);
        sfxToggle?.onValueChanged.AddListener(OnSFXToggled);
        notificationsToggle?.onValueChanged.AddListener(OnNotificationsToggled);

        closeButton?.onClick.AddListener(OnClose);
        resetProgressButton?.onClick.AddListener(OnResetPressed);
        confirmResetButton?.onClick.AddListener(OnConfirmReset);
        cancelResetButton?.onClick.AddListener(() => resetConfirmPanel?.SetActive(false));
        privacyPolicyButton?.onClick.AddListener(() => Application.OpenURL("https://example.com/privacy"));
        termsButton?.onClick.AddListener(() => Application.OpenURL("https://example.com/terms"));

        if (versionText != null)
            versionText.text = $"v{Application.version}";

        resetConfirmPanel?.SetActive(false);
    }

    private void LoadSettings()
    {
        if (musicSlider != null) musicSlider.value = SoundManager.Instance?.MusicVolume ?? (GameData.MusicEnabled ? 1f : 0f);
        if (sfxSlider   != null) sfxSlider.value   = SoundManager.Instance?.SFXVolume   ?? (GameData.SFXEnabled   ? 1f : 0f);
        if (musicToggle != null) musicToggle.isOn   = GameData.MusicEnabled;
        if (sfxToggle   != null) sfxToggle.isOn     = GameData.SFXEnabled;
        if (notificationsToggle != null) notificationsToggle.isOn = GameData.NotificationsEnabled;

        RefreshVolumeLabels();
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.MusicVolume = value;
        RefreshVolumeLabels();
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SFXVolume = value;
        RefreshVolumeLabels();
    }

    private void OnMusicToggled(bool on)
    {
        GameData.MusicEnabled = on;
        if (on)  SoundManager.Instance?.PlayMenuMusic();
        else     SoundManager.Instance?.StopMusic();
    }

    private void OnSFXToggled(bool on)
    {
        GameData.SFXEnabled = on;
    }

    private void OnNotificationsToggled(bool on)
    {
        GameData.NotificationsEnabled = on;
        GameData.Save();
    }

    private void RefreshVolumeLabels()
    {
        if (musicValueText != null && musicSlider != null)
            musicValueText.text = Mathf.RoundToInt(musicSlider.value * 100) + "%";
        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    private void OnClose()
    {
        SoundManager.Instance?.PlayButtonClick();
        GameData.Save();
        UIManager.Instance?.GoToMainMenu();
    }

    private void OnResetPressed()
    {
        SoundManager.Instance?.PlayButtonClick();
        resetConfirmPanel?.SetActive(true);
    }

    private void OnConfirmReset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        resetConfirmPanel?.SetActive(false);
        UIManager.Instance?.GoToMainMenu();
    }
}
