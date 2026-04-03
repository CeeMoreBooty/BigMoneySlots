using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup that lets the player set/change their display name.
/// Shown on first boot after the loading screen.
/// </summary>
public class PlayerNameController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject       panel;
    [SerializeField] private TMP_InputField   nameInput;
    [SerializeField] private Button           confirmButton;
    [SerializeField] private Button           closeButton;
    [SerializeField] private TextMeshProUGUI  errorText;
    [SerializeField] private int              maxLength = 16;

    private const string NAME_KEY = "PlayerName";

    private void Start()
    {
        confirmButton?.onClick.AddListener(OnConfirm);
        closeButton?.onClick.AddListener(Hide);
        panel?.SetActive(false);

        if (!PlayerPrefs.HasKey(NAME_KEY))
            Show();
    }

    public void Show()
    {
        if (nameInput != null)
            nameInput.text = PlayerPrefs.GetString(NAME_KEY, string.Empty);
        if (errorText != null) errorText.text = string.Empty;
        panel?.SetActive(true);
    }

    public void Hide()
    {
        panel?.SetActive(false);
        SoundManager.Instance?.PlayButtonClick();
    }

    private void OnConfirm()
    {
        string name = nameInput?.text?.Trim() ?? string.Empty;

        if (name.Length < 2)
        {
            if (errorText != null) errorText.text = "Name must be at least 2 characters.";
            return;
        }
        if (name.Length > maxLength)
        {
            if (errorText != null) errorText.text = $"Name must be {maxLength} characters or fewer.";
            return;
        }

        PlayerPrefs.SetString(NAME_KEY, name);
        PlayerPrefs.Save();
        SoundManager.Instance?.PlayButtonClick();
        Hide();
    }
}
