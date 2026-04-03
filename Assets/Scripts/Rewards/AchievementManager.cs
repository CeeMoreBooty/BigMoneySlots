using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Milestone-based achievement system. Achievements are checked whenever
/// GameData changes; unlocking is permanent (never resets).
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // ── Achievement definition ────────────────────────────────────────────────

    [Serializable]
    public class Achievement
    {
        public string  id;
        public string  displayName;
        public string  description;
        public Sprite  icon;
        public long    target;         // numeric threshold
        public string  category;       // "Spins"|"Wins"|"Coins"|"BigWin"|"Jackpot"
        public long    rewardCoins;
        public bool    Unlocked => PlayerPrefs.GetInt($"Ach_{id}", 0) == 1;

        public void Unlock()
        {
            PlayerPrefs.SetInt($"Ach_{id}", 1);
            PlayerPrefs.Save();
        }
    }

    // ── Toast UI ──────────────────────────────────────────────────────────────

    [Header("Toast Notification")]
    [SerializeField] private GameObject       toastPanel;
    [SerializeField] private TextMeshProUGUI  toastNameText;
    [SerializeField] private TextMeshProUGUI  toastDescText;
    [SerializeField] private Image            toastIcon;
    [SerializeField] private Animator         toastAnimator;
    [SerializeField] private float            toastDuration = 3f;

    [Header("Achievement List Panel")]
    [SerializeField] private GameObject  listPanel;
    [SerializeField] private Transform   listContainer;
    [SerializeField] private GameObject  achievementRowPrefab;
    [SerializeField] private Button      listCloseButton;

    [Header("Achievements")]
    [SerializeField] private Achievement[] achievements;

    private static readonly int ToastIn  = Animator.StringToHash("SlideIn");
    private static readonly int ToastOut = Animator.StringToHash("SlideOut");

    private Queue<Achievement> toastQueue = new Queue<Achievement>();
    private bool toastActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        listCloseButton?.onClick.AddListener(HideList);
        toastPanel?.SetActive(false);
        listPanel?.SetActive(false);
    }

    // ── Public check API (call after relevant GameData changes) ───────────────

    public void CheckSpins()  => CheckCategory("Spins",   GameData.TotalSpins);
    public void CheckWins()   => CheckCategory("Wins",    GameData.TotalWins);
    public void CheckCoins()  => CheckCategory("Coins",   GameData.Coins);
    public void CheckBigWin(long amount) => CheckCategory("BigWin", amount);
    public void CheckJackpot()           => CheckCategory("Jackpot", 1);

    private void CheckCategory(string category, long value)
    {
        foreach (var ach in achievements)
        {
            if (ach.category != category) continue;
            if (ach.Unlocked) continue;
            if (value >= ach.target) TriggerUnlock(ach);
        }
    }

    private void TriggerUnlock(Achievement ach)
    {
        ach.Unlock();
        GameData.AddCoins(ach.rewardCoins);
        GameData.Save();
        toastQueue.Enqueue(ach);
        if (!toastActive) StartCoroutine(ShowToastQueue());
    }

    private System.Collections.IEnumerator ShowToastQueue()
    {
        toastActive = true;
        while (toastQueue.Count > 0)
        {
            var ach = toastQueue.Dequeue();
            if (toastNameText != null) toastNameText.text = ach.displayName;
            if (toastDescText != null) toastDescText.text = $"{ach.description}\n+{CoinDisplay.FormatCoins(ach.rewardCoins)} coins";
            if (toastIcon     != null && ach.icon != null) toastIcon.sprite = ach.icon;

            toastPanel?.SetActive(true);
            toastAnimator?.SetTrigger(ToastIn);
            yield return new WaitForSeconds(toastDuration);
            toastAnimator?.SetTrigger(ToastOut);
            yield return new WaitForSeconds(0.5f);
            toastPanel?.SetActive(false);
        }
        toastActive = false;
    }

    // ── Achievement list panel ────────────────────────────────────────────────

    public void ShowList()
    {
        listPanel?.SetActive(true);
        PopulateList();
    }

    public void HideList()
    {
        listPanel?.SetActive(false);
        SoundManager.Instance?.PlayButtonClick();
    }

    private void PopulateList()
    {
        if (listContainer == null || achievementRowPrefab == null) return;

        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        foreach (var ach in achievements)
        {
            var go   = Instantiate(achievementRowPrefab, listContainer);
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = ach.displayName;
            if (texts.Length > 1) texts[1].text = ach.description;
            if (texts.Length > 2) texts[2].text = ach.Unlocked ? "✔ Unlocked" : $"Goal: {CoinDisplay.FormatCoins(ach.target)}";

            var img = go.GetComponentInChildren<Image>();
            if (img != null && ach.icon != null) img.sprite = ach.icon;

            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            cg.alpha = ach.Unlocked ? 1f : 0.5f;
        }
    }
}
