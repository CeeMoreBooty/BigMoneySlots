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
    [SerializeField] private Button changeProfileImageButton;

    [Header("Profile Image")]
    [SerializeField] private Image currentProfileImage;
    [SerializeField] private GameObject profileImagePanel;
    [SerializeField] private Button confirmImageButton;
    [SerializeField] private Button cancelImageButton;
    [SerializeField] private TMP_InputField imageUrlInput;

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

        changeProfileImageButton?.onClick.AddListener(OnChangeProfileImage);
        confirmImageButton?.onClick.AddListener(OnConfirmProfileImage);
        cancelImageButton?.onClick.AddListener(() => profileImagePanel?.SetActive(false));

        if (versionText != null)
            versionText.text = $"v{Application.version}";

        resetConfirmPanel?.SetActive(false);
        profileImagePanel?.SetActive(false);

        // Load current profile image
        RefreshProfileImage();
    }

    private void LoadSettings()
    {
        if (musicSlider != null) musicSlider.value = SoundManager.Instance?.MusicVolume ?? GameData.MusicEnabled ? 1f : 0f;
        if (sfxSlider   != null) sfxSlider.value   = SoundManager.Instance?.SFXVolume   ?? GameData.SFXEnabled   ? 1f : 0f;
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

    private void OnChangeProfileImage()
    {
        SoundManager.Instance?.PlayButtonClick();
        profileImagePanel?.SetActive(true);

        // Pre-fill with current URL
        if (imageUrlInput != null && PlayerEconomy.Instance != null)
        {
            imageUrlInput.text = PlayerEconomy.Instance.ProfileImageUrl ?? "";
        }
    }

    private void OnConfirmProfileImage()
    {
        SoundManager.Instance?.PlayButtonClick();

        if (imageUrlInput == null) return;
        string newUrl = imageUrlInput.text?.Trim() ?? "";

        // Update locally
        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.SetProfileImage(newUrl);
        }

        // Update backend
        StartCoroutine(UpdateProfileImageOnBackend(newUrl));

        profileImagePanel?.SetActive(false);
        RefreshProfileImage();
    }

    private void RefreshProfileImage()
    {
        if (currentProfileImage != null && ImageCache.Instance != null && PlayerEconomy.Instance != null)
        {
            string url = PlayerEconomy.Instance.ProfileImageUrl;
            ImageCache.Instance.LoadImageToUI(url, currentProfileImage, true);
        }
    }

    private System.Collections.IEnumerator UpdateProfileImageOnBackend(string imageUrl)
    {
        if (BackendClient.Instance == null || string.IsNullOrEmpty(BackendClient.AuthToken))
        {
            Debug.LogWarning("Not authenticated - cannot update profile image on backend");
            yield break;
        }

        string json = JsonUtility.ToJson(new ProfileImageUpdate { profileImageUrl = imageUrl });
        using (var req = UnityEngine.Networking.UnityWebRequest.Put($"{BackendClient.BaseUrl}/api/user/profile-image", json))
        {
            req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Profile image updated successfully on backend");
            }
            else
            {
                Debug.LogWarning($"Failed to update profile image on backend: {req.error}");
            }
        }
    }

    [System.Serializable]
    private class ProfileImageUpdate
    {
        public string profileImageUrl;
    }
}
