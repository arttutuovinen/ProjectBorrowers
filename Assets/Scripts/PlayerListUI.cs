using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerListUI : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public TextMeshProUGUI playerListText;
    public GameObject panel;

    [Header("Behavior")]
    [Tooltip("If true, panel is always visible (Lobby use case).")]
    public bool alwaysVisible = false;

    [Tooltip("Optional debug logging.")]
    public bool verboseDebugLogs = false;

    void Start()
    {
        if (panel == null)
            panel = gameObject;

        if (alwaysVisible)
            panel.SetActive(true);
        else
            panel.SetActive(false);

        RefreshPlayerList();

        if (verboseDebugLogs)
            Debug.Log("[PlayerListUI] Started. AlwaysVisible: " + alwaysVisible);
    }

    void Update()
    {
        if (alwaysVisible)
            return;

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            TryToggleFromInput();
        }
    }

    private void TryToggleFromInput()
    {
        if (IsTypingInInputField())
            return;

        panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf)
            RefreshPlayerList();
    }

    private bool IsTypingInInputField()
    {
        var es = EventSystem.current;
        if (es == null) return false;

        var selected = es.currentSelectedGameObject;
        if (selected == null) return false;

        return selected.GetComponent<TMP_InputField>() != null ||
               selected.GetComponent<UnityEngine.UI.InputField>() != null;
    }

    void RefreshPlayerList()
    {
        if (playerListText == null) return;

        playerListText.text = "";

        foreach (var p in PhotonNetwork.PlayerList)
        {
            string displayName =
    p.CustomProperties.TryGetValue(LobbyKeys.PlayerName, out object nameObj)
        ? nameObj.ToString()
        : !string.IsNullOrEmpty(p.NickName)
            ? p.NickName
            : $"Player {p.ActorNumber}";


            // Add Role
            string roleText = "";
            if (p.CustomProperties.TryGetValue(LobbyKeys.PlayerRole, out object roleObj))
            {
                int role = (int)roleObj;
                roleText = role == 0 ? " [BigPlayer]" : " [SmallPlayer]";
            }
            playerListText.text += $"• {displayName}{roleText}\n";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerList();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Refresh player list when any player property changes
        RefreshPlayerList();
    }

}
