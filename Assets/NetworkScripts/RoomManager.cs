using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // using TextMeshPro InputField

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    public Transform spawnPoint;

    [Header("UI (hook these in Inspector)")]
    public TMP_InputField nameInput;           // NameInput from your LobbyPanel
    public GameObject[] characterButtons;      // CharButton_0, CharButton_1 assigned in Inspector
    public TMPro.TextMeshProUGUI joinButtonText; // optional label to show status
    public GameObject lobbyPanel;              // assign your LobbyPanel here

    // 0 = SmallPlayer, 1 = BigPlayer
    int selectedCharacterIndex = 0;

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
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions { MaxPlayers = 8 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        string prefabName = GetPrefabNameForIndex(selectedCharacterIndex);
        Debug.Log("Joined Room. Instantiating player prefab: " + prefabName);

        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("No prefab name mapped for selection index: " + selectedCharacterIndex);
            return;
        }

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

        // ===== Hide the lobby UI for this client =====
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        // Optional: lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected: " + cause);
        if (joinButtonText != null) joinButtonText.text = "Join";

        // If disconnected while in game, show lobby again
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
