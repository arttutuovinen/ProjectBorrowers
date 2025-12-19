using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    public Transform[] smallPlayerSpawnPoints;   // <-- Multiple SP spawn points
    public Transform bigPlayerSpawnPoint;        // <-- Single BP spawn point
    public Transform[] prisonSpawnPoints;   // <-- Prison spawn locations

    [Header("UI (hook these in Inspector)")]
    public TMP_InputField nameInput;
    public GameObject[] characterButtons;
    public TMPro.TextMeshProUGUI joinButtonText;
    public GameObject lobbyPanel;

    int selectedCharacterIndex = 0; // 0 = SmallPlayer, 1 = BigPlayer

    // ---------- UI callbacks ----------
    public void OnNameInputChanged(string newName) { }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index > 1) return;
        selectedCharacterIndex = index;
        UpdateCharacterButtonVisuals();
    }

    void UpdateCharacterButtonVisuals()
    {
        if (characterButtons == null) return;
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] == null) continue;
            var img = characterButtons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = (i == selectedCharacterIndex) ? Color.green : Color.white;
        }
    }

    // Called by JoinButton OnClick
    public void OnJoinButtonPressed()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.Log("Name is required before joining a room!");
            if (joinButtonText != null) joinButtonText.text = "Enter a Name!";
            return;
        }

        PhotonNetwork.NickName = nameInput.text.Trim();
        if (joinButtonText != null) joinButtonText.text = "Connecting...";
        PhotonNetwork.ConnectUsingSettings();
    }

    // ---------- Photon callbacks ----------
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master. Joining/creating room...");
        PhotonNetwork.JoinOrCreateRoom(
            "TestRoom",
            new RoomOptions { MaxPlayers = 8 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log(
    $"Joined room '{PhotonNetwork.CurrentRoom.Name}' | " +
    $"Players: {PhotonNetwork.PlayerList.Length} | " +
    $"Region: {PhotonNetwork.CloudRegion} | " +
    $"GameVersion: {PhotonNetwork.GameVersion}"
);
        string prefabName = GetPrefabNameForIndex(selectedCharacterIndex);
        Debug.Log("Joined Room. Instantiating player prefab: " + prefabName);

        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("No prefab name mapped for selection index: " + selectedCharacterIndex);
            return;
        }

        // ------------ Spawn Point Selection ------------
        Transform spawnPoint = null;

        if (selectedCharacterIndex == 0) // SmallPlayer
        {
            if (smallPlayerSpawnPoints != null && smallPlayerSpawnPoints.Length > 0)
            {
                int rand = Random.Range(0, smallPlayerSpawnPoints.Length);
                spawnPoint = smallPlayerSpawnPoints[rand];
            }
        }
        else if (selectedCharacterIndex == 1) // BigPlayer
        {
            spawnPoint = bigPlayerSpawnPoint;
        }

        Vector3 spawnPos =
            (spawnPoint != null) ? spawnPoint.position : Vector3.zero;

        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

        // ---- Prison Spawn (ONLY ONCE) ----
        if (PhotonNetwork.IsMasterClient)
        {
            if (prisonSpawnPoints != null && prisonSpawnPoints.Length > 0)
            {
                int rand = Random.Range(0, prisonSpawnPoints.Length);
                PhotonNetwork.Instantiate(
                    "Prison",
                    prisonSpawnPoints[rand].position,
                    Quaternion.identity
                );
            }
        }

        // Hide Lobby UI
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected: " + cause);
        if (joinButtonText != null) joinButtonText.text = "Join";

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    private string GetPrefabNameForIndex(int index)
    {
        switch (index)
        {
            case 0: return "SmallPlayer";
            case 1: return "BigPlayer";
            default: return null;
        }
    }
}


