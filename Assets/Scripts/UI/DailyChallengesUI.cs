using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for displaying and claiming daily challenges.
/// Reads challenge data from DailyChallenges.TodayChallenges list.
/// </summary>
public class DailyChallengesUI : MonoBehaviour
{
    [SerializeField] private GameObject challengeRowPrefab;
    [SerializeField] private Transform  rowContainer;

    private DailyChallenges challenges;

    private void Start()
    {
        challenges = FindObjectOfType<DailyChallenges>();
        Refresh();
    }

    public void Refresh()
    {
        if (challenges == null || rowContainer == null) return;

        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        var list = challenges.TodayChallenges;
        for (int i = 0; i < list.Count; i++)
        {
            int index = i; // capture for lambda
            var c = list[i];
            long current   = c.progress;
            long target    = c.target;
            bool claimable = challenges.IsClaimable(i);

            if (challengeRowPrefab == null) continue;

            GameObject row = Instantiate(challengeRowPrefab, rowContainer);

            var desc = row.transform.Find("DescText")?.GetComponent<TMP_Text>();
            if (desc) desc.text = c.description;

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
        var list = challenges?.TodayChallenges;
        if (list == null || index < 0 || index >= list.Count) return;

        bool claimed = challenges.ClaimReward(list[index].id);
        if (claimed) Refresh();
    }
}
