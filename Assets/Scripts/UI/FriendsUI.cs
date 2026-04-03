using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

/// <summary>UI panel for the Friends system.</summary>
public class FriendsUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Tabs")]
    public Button tabFriends;
    public Button tabPending;
    public Button tabSearch;

    [Header("Content")]
    public Transform friendsListParent;
    public Transform pendingListParent;
    public GameObject friendRowPrefab;    // assign prefab in Inspector

    [Header("Search")]
    public TMP_InputField searchInput;
    public Button          searchButton;
    public Transform       searchResultsParent;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    private enum Tab { Friends, Pending, Search }
    private Tab _activeTab = Tab.Friends;

    private void Awake()
    {
        tabFriends?.onClick.AddListener(() => SwitchTab(Tab.Friends));
        tabPending?.onClick.AddListener(() => SwitchTab(Tab.Pending));
        tabSearch?.onClick.AddListener(()  => SwitchTab(Tab.Search));
        searchButton?.onClick.AddListener(OnSearchClicked);
    }

    private void OnEnable()
    {
        FriendsManager.OnFriendsUpdated      += Refresh;
        FriendsManager.OnFriendRequestReceived += n => SetFeedback($"📨 Friend request from {n}!");
        Refresh();
    }

    private void OnDisable() => FriendsManager.OnFriendsUpdated -= Refresh;

    public void Show() { panel?.SetActive(true); Refresh(); }
    public void Hide() => panel?.SetActive(false);

    private void SwitchTab(Tab t)
    {
        _activeTab = t;
        friendsListParent?.gameObject.SetActive(t == Tab.Friends);
        pendingListParent?.gameObject.SetActive(t == Tab.Pending);
        searchResultsParent?.gameObject.SetActive(t == Tab.Search);
        Refresh();
    }

    private void Refresh()
    {
        if (FriendsManager.Instance == null) return;
        PopulateList(friendsListParent, FriendsManager.Instance.Friends, false);
        PopulateList(pendingListParent, FriendsManager.Instance.Pending,  true);
    }

    private void PopulateList(Transform parent, List<FriendsManager.Friend> list, bool isPending)
    {
        if (parent == null) return;
        foreach (Transform child in parent) Destroy(child.gameObject);

        foreach (var f in list)
        {
            if (friendRowPrefab == null) break;
            var row  = Instantiate(friendRowPrefab, parent);
            var name = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
            var status = row.transform.Find("StatusText")?.GetComponent<TMP_Text>();
            var acceptBtn = row.transform.Find("AcceptButton")?.GetComponent<Button>();
            var removeBtn = row.transform.Find("RemoveButton")?.GetComponent<Button>();
            var dmBtn     = row.transform.Find("DMButton")?.GetComponent<Button>();

            if (name   != null) name.text   = f.displayName;
            if (status != null) status.text = f.isOnline ? "🟢 Online" : "⚫ Offline";

            string id       = f.playerId;
            string dName    = f.displayName;
            if (isPending)
            {
                acceptBtn?.gameObject.SetActive(true);
                removeBtn?.gameObject.SetActive(true);
                dmBtn?.gameObject.SetActive(false);
                acceptBtn?.onClick.AddListener(() => { FriendsManager.Instance?.AcceptFriendRequest(id); });
                removeBtn?.onClick.AddListener(() => { FriendsManager.Instance?.DeclineFriendRequest(id); });
            }
            else
            {
                acceptBtn?.gameObject.SetActive(false);
                dmBtn?.gameObject.SetActive(true);
                removeBtn?.onClick.AddListener(() => { FriendsManager.Instance?.RemoveFriend(id); });
                dmBtn?.onClick.AddListener(() => { DirectMessageUI.Instance?.OpenThread(id, dName); });
            }
        }
    }

    private void OnSearchClicked()
    {
        string query = searchInput?.text?.Trim();
        if (string.IsNullOrEmpty(query)) return;
        SetFeedback($"Searching for \"{query}\"…");
        StartCoroutine(SearchPlayers(query));
    }

    private IEnumerator SearchPlayers(string query)
    {
        string url = $"{BackendClient.BaseUrl}/api/friends/search?q={UnityEngine.Networking.UnityWebRequest.EscapeURL(query)}";
        using (var req = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Authorization", $"Bearer {BackendClient.AuthToken}");
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var wrapper = JsonUtility.FromJson<SearchWrapper>(req.downloadHandler.text);
                if (searchResultsParent != null)
                {
                    foreach (Transform child in searchResultsParent) Destroy(child.gameObject);
                    foreach (var p in wrapper.results)
                    {
                        if (friendRowPrefab == null) break;
                        var row       = Instantiate(friendRowPrefab, searchResultsParent);
                        var nameText  = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
                        var addBtn    = row.transform.Find("AcceptButton")?.GetComponent<Button>();
                        if (nameText != null) nameText.text = p.displayName;
                        string id = p.playerId;
                        addBtn?.gameObject.SetActive(true);
                        addBtn?.onClick.AddListener(() =>
                        {
                            FriendsManager.Instance?.SendFriendRequest(id);
                            SetFeedback($"Friend request sent to {p.displayName}!");
                        });
                    }
                }
                SetFeedback(wrapper.results.Count == 0 ? "No players found." : $"Found {wrapper.results.Count} player(s).");
            }
            else
            {
                SetFeedback("Search failed. Please try again.");
            }
        }
    }

    [System.Serializable]
    private class SearchWrapper
    {
        public List<SearchResult> results;
    }

    [System.Serializable]
    private class SearchResult
    {
        public string playerId;
        public string displayName;
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
    }
}
