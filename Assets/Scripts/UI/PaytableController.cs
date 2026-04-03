using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Paytable scene: shows payout tables, payline diagrams, symbol legend.
/// </summary>
public class PaytableController : MonoBehaviour
{
    [System.Serializable]
    public class PaytableRow
    {
        public Image             symbolIcon;
        public TextMeshProUGUI   symbolNameText;
        public TextMeshProUGUI   payout3Text;
        public TextMeshProUGUI   payout4Text;
        public TextMeshProUGUI   payout5Text;
    }

    [Header("Config")]
    [SerializeField] private SlotConfig slotConfig;

    [Header("Paytable Rows")]
    [SerializeField] private PaytableRow[] rows;

    [Header("Payline Display")]
    [SerializeField] private Transform  paylineContainer;
    [SerializeField] private GameObject paylinePrefab;

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    private void Start()
    {
        closeButton?.onClick.AddListener(OnClose);
        PopulateTable();
        PopulatePaylines();
    }

    private void PopulateTable()
    {
        if (slotConfig?.symbols == null) return;

        for (int i = 0; i < rows.Length && i < slotConfig.symbols.Length; i++)
        {
            var sym = slotConfig.symbols[i];
            var row = rows[i];

            if (row.symbolIcon    != null && sym.sprite != null) row.symbolIcon.sprite = sym.sprite;
            if (row.symbolNameText != null) row.symbolNameText.text = sym.symbolName;
            if (row.payout3Text   != null) row.payout3Text.text   = $"{sym.payout3}×";
            if (row.payout4Text   != null) row.payout4Text.text   = $"{sym.payout4}×";
            if (row.payout5Text   != null) row.payout5Text.text   = $"{sym.payout5}×";
        }
    }

    private void PopulatePaylines()
    {
        if (slotConfig?.paylines == null || paylineContainer == null || paylinePrefab == null) return;

        foreach (Transform child in paylineContainer)
            Destroy(child.gameObject);

        foreach (var payline in slotConfig.paylines)
        {
            var go = Instantiate(paylinePrefab, paylineContainer);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = payline.paylineName;

            var img = go.GetComponent<Image>();
            if (img != null) img.color = payline.lineColor;
        }
    }

    private void OnClose()
    {
        SoundManager.Instance?.PlayButtonClick();
        UIManager.Instance?.GoToGame();
    }
}
