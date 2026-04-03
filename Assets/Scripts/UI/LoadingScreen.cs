using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Reusable loading overlay that can be triggered during async operations.
/// Call Show() before a heavy operation and Hide() when done.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject       panel;
    [SerializeField] private Slider           progressBar;
    [SerializeField] private TextMeshProUGUI  messageText;
    [SerializeField] private Image            spinnerImage;
    [SerializeField] private float            spinSpeed = 360f;

    private bool spinning;
    private Coroutine spinCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        panel?.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show(string message = "Loading...")
    {
        if (messageText != null) messageText.text = message;
        if (progressBar != null) { progressBar.gameObject.SetActive(false); }
        panel?.SetActive(true);
        StartSpinner();
    }

    public void ShowWithProgress(string message = "Loading...")
    {
        if (messageText != null) messageText.text = message;
        if (progressBar != null) { progressBar.value = 0; progressBar.gameObject.SetActive(true); }
        panel?.SetActive(true);
        StartSpinner();
    }

    public void SetProgress(float value, string message = null)
    {
        if (progressBar != null) progressBar.value = Mathf.Clamp01(value);
        if (message != null && messageText != null) messageText.text = message;
    }

    public void Hide()
    {
        panel?.SetActive(false);
        StopSpinner();
    }

    // ── Spinner ───────────────────────────────────────────────────────────────

    private void StartSpinner()
    {
        if (spinCoroutine != null) StopCoroutine(spinCoroutine);
        if (spinnerImage  != null) spinCoroutine = StartCoroutine(SpinRoutine());
    }

    private void StopSpinner()
    {
        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }
    }

    private IEnumerator SpinRoutine()
    {
        while (true)
        {
            spinnerImage.transform.Rotate(0, 0, -spinSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
