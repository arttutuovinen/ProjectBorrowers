using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerListUI : MonoBehaviourPunCallbacks
{
    [Tooltip("Drag the TextMeshProUGUI here (child of panel).")]
    public TextMeshProUGUI playerListText;

    [Tooltip("The panel GameObject that will be toggled (assign PlayerListPanel).")]
    public GameObject panel;

    [Tooltip("Optional: log debug info to console each frame (set false to reduce spam).")]
    public bool verboseDebugLogs = true;

    void Start()
    {
        if (panel == null) panel = gameObject;
        panel.SetActive(false);
        RefreshPlayerList();
        Debug.Log("[PlayerListUI] Start() - panel assigned: " + (panel != null) + ", text assigned: " + (playerListText != null));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            TryToggleFromInput();
            return; // ensures instant toggle
        }

        if (verboseDebugLogs && Time.frameCount % 60 == 0)
            Debug.Log("[PlayerListUI] Update() running. Selected: " + GetSelectedObjectName());
    }


    

    private void TryToggleFromInput()
    {
        if (IsTypingInInputField())
        {
            Debug.Log("[PlayerListUI] Ignoring toggle because an input field is focused: " + GetSelectedObjectName());
            return;
        }

        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) RefreshPlayerList();
        Debug.Log("[PlayerListUI] Panel toggled. NowActive: " + panel.activeSelf);
    }

    public void TogglePanel() => TryToggleFromInput();

    private string GetSelectedObjectName()
    {
        var es = EventSystem.current;
        if (es == null) return "No EventSystem";
        var sel = es.currentSelectedGameObject;
        return sel != null ? sel.name : "None";
    }

    private bool IsTypingInInputField()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        var selected = es.currentSelectedGameObject;
        if (selected == null) return false;
        if (selected.GetComponent<TMP_InputField>() != null) return true;
        if (selected.GetComponent<UnityEngine.UI.InputField>() != null) return true;
        return false;
    }

    void RefreshPlayerList()
    {
        if (playerListText == null)
        {
            Debug.LogWarning("[PlayerListUI] Refresh called but playerListText is null!");
            return;
        }

        playerListText.text = "Players in Room:\n";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            string displayName = string.IsNullOrEmpty(p.NickName) ? $"Player {p.ActorNumber}" : p.NickName;
            playerListText.text += "• " + displayName + "\n";
        }

        if (verboseDebugLogs) Debug.Log("[PlayerListUI] Refreshed list with " + PhotonNetwork.PlayerList.Length + " players.");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) { RefreshPlayerList(); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { RefreshPlayerList(); }
}
