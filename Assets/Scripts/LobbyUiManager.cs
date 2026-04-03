using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LobbyUiManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField nameInput;
    public Button bigPlayerButton;
    public Button smallPlayerButton;
    public TMP_Text spAmountText;
    public TMP_Text bpAmountText;

    [Header("Full Indicators")]
    public GameObject spAmountFull;
    public GameObject bpAmountFull;

    void Awake()
    {
        // ✅ Always restore cursor when entering LobbyScene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        // Clear stale properties from previous session
        Hashtable props = new Hashtable
        {
            { LobbyKeys.PlayerRole, null },
            { LobbyKeys.PlayerName, null }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        roomCodeText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";

        smallPlayerButton.onClick.AddListener(() => SelectRole(1));
        bigPlayerButton.onClick.AddListener(() => SelectRole(0));
        ResetRoleSelectionUI();
        RefreshRoleUI();
    }

    void SelectRole(int role)
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
            return;
        string name = nameInput.text.Trim();
        PhotonNetwork.NickName = name;

        // Prevent overfilling
        if (!CanTakeRole(role))
            return;

        var props = new Hashtable
        {
            { LobbyKeys.PlayerRole, role },
            { LobbyKeys.PlayerName, name }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    bool CanTakeRole(int role)
    {
        int count = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue(LobbyKeys.PlayerRole, out object r))
            {
                if ((int)r == role)
                    count++;
            }
        }

        if (role == 0) return count < RequiredPlayeramount.RequiredBigPlayers;
        return count < RequiredPlayeramount.RequiredSmallPlayers;
    }

    void RefreshRoleUI()
    {
        int spCount = 0;
        int bpCount = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.TryGetValue(LobbyKeys.PlayerRole, out object r))
                continue;

            if ((int)r == 0) bpCount++;
            else spCount++;
        }

        spAmountText.text = $"{spCount}/{RequiredPlayeramount.RequiredSmallPlayers}";
        bpAmountText.text = $"{bpCount}/{RequiredPlayeramount.RequiredBigPlayers}";

        smallPlayerButton.image.color =
            spCount >= RequiredPlayeramount.RequiredSmallPlayers ? Color.black : Color.white;

        bigPlayerButton.image.color =
            bpCount >= RequiredPlayeramount.RequiredBigPlayers ? Color.black : Color.white;

        spAmountFull.SetActive(spCount >= RequiredPlayeramount.RequiredSmallPlayers);
        bpAmountFull.SetActive(bpCount >= RequiredPlayeramount.RequiredBigPlayers);
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        RefreshRoleUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshRoleUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshRoleUI();
    }
    void ResetRoleSelectionUI()
    {
        Hashtable props = new Hashtable
    {
        { LobbyKeys.PlayerRole, null },
        { LobbyKeys.PlayerName, null }
    };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        smallPlayerButton.image.color = Color.white;
        bigPlayerButton.image.color = Color.white;

        spAmountFull.SetActive(false);
        bpAmountFull.SetActive(false);
    }

    public void OnLeaveButtonPressed()
    {
        // Restore cursor locally
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Leave room (works for any client)
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Disconnect from Photon completely
        PhotonNetwork.Disconnect();

        // Go back to Main Menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
