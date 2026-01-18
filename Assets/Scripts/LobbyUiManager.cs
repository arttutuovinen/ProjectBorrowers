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

    [Header("Role Limits")]
    public int maxSmallPlayers = 2;
    public int maxBigPlayers = 1;

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

        if (role == 0) return count < maxBigPlayers;
        return count < maxSmallPlayers;
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

        spAmountText.text = $"{spCount}/{maxSmallPlayers}";
        bpAmountText.text = $"{bpCount}/{maxBigPlayers}";

        smallPlayerButton.image.color =
            spCount >= maxSmallPlayers ? Color.black : Color.white;

        bigPlayerButton.image.color =
            bpCount >= maxBigPlayers ? Color.black : Color.white;

        spAmountFull.SetActive(spCount >= maxSmallPlayers);
        bpAmountFull.SetActive(bpCount >= maxBigPlayers);
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
        // Clear role selection locally
        var localProps = PhotonNetwork.LocalPlayer.CustomProperties;
        if (localProps.ContainsKey(LobbyKeys.PlayerRole))
            localProps.Remove(LobbyKeys.PlayerRole);

        PhotonNetwork.LocalPlayer.SetCustomProperties(localProps);

        // Reset buttons colors
        smallPlayerButton.image.color = Color.white;
        bigPlayerButton.image.color = Color.white;

        // Reset full indicators
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
