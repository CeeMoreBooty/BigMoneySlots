using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for displaying and claiming daily challenges.
/// </summary>
public class DailyChallengesUI : MonoBehaviour
{
    [SerializeField] private GameObject challengeRowPrefab;
    [SerializeField] private Transform  rowContainer;

    private DailyChallenges challenges;

    private void Start()
    {
        challenges = FindObjectOfType<DailyChallenges>();
        if (challenges == null) return;
        Refresh();
    }

    public void Refresh()
    {
        if (challenges == null || rowContainer == null) return;

        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < challenges.ChallengeCount; i++)
        {
            int index = i; // capture for lambda
            var (current, target) = challenges.GetProgress(i);
            bool claimable = challenges.IsClaimable(i);

            if (challengeRowPrefab == null) continue;

            GameObject row = Instantiate(challengeRowPrefab, rowContainer);

            var desc = row.transform.Find("DescText")?.GetComponent<TMP_Text>();
            if (desc) desc.text = challenges.GetDescription(i);

            var prog = row.transform.Find("ProgressText")?.GetComponent<TMP_Text>();
            if (prog) prog.text = $"{current}/{target}";

            var slider = row.transform.Find("ProgressBar")?.GetComponent<Slider>();
            if (slider) slider.value = target > 0 ? (float)current / target : 0f;

            var btn = row.transform.Find("ClaimButton")?.GetComponent<Button>();
            if (btn)
            {
                btn.interactable = claimable;
                btn.onClick.AddListener(() => OnClaim(index));
            }
        }
    }

    private void OnClaim(int index)
    {
        var (coins, gems) = challenges.ClaimChallenge(index);
        if (coins > 0 || gems > 0)
        {
            var ud = GameManager.Instance?.userData;
            ud?.AddCoins(coins);
            if (gems > 0) ud?.AddGems(gems);
            Refresh();
        }
    }
}
