using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    public Transform[] spFinishSpawnPoints;      // 10 Finish spawn points
    public int finishCount = 5;
    public Transform bigPlayerSpawnPoint;
    public Transform[] prisonSpawnPoints;

    [Header("UI (hook these in Inspector)")]
    public TMP_InputField nameInput;
    public GameObject[] characterButtons;
    public TextMeshProUGUI joinButtonText;
    public GameObject lobbyPanel;

    int selectedCharacterIndex = 0; // 0 = SmallPlayer, 1 = BigPlayer

    // Prevent multiple leave triggers
    private bool isLeaving = false;

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

            var button = characterButtons[i].GetComponent<Button>();
            if (button == null) continue;

            // Enable interactable state if needed (optional)
            button.interactable = true;

            // Use the normal color for unselected, and just set the 'selected' state via a color tint
            var colors = button.colors;

            if (i == selectedCharacterIndex)
            {
                // Highlight the selected button by changing only the normal color
                colors.normalColor = colors.pressedColor; // or a custom selected color
            }
            else
            {
                // Reset unselected buttons to their default normal color
                colors.normalColor = Color.white;
            }

            button.colors = colors;
        }
    }


    // ---------- Join ----------
    public void OnJoinButtonPressed()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text))
        {
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

        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.JoinOrCreateRoom(
            "TestRoom",
            new RoomOptions { MaxPlayers = 8 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // ---------- Spawn Finish & Prison (ONCE) ----------
        if (PhotonNetwork.IsMasterClient)
        {
            // Spawn Finish prefabs
            var selectedPoints = spFinishSpawnPoints
                .OrderBy(x => Random.value)
                .Take(finishCount);

            foreach (var point in selectedPoints)
            {
                PhotonNetwork.Instantiate(
                    "SPFinish",
                    point.position,
                    point.rotation
                );
            }

            // Spawn Prison
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

        // ---------- Spawn Player ----------
        StartCoroutine(SpawnPlayerAfterFinishesReady());

        // ---------- UI ----------
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator SpawnPlayerAfterFinishesReady()
    {
        // SmallPlayer needs Finish prefabs to exist
        if (selectedCharacterIndex == 0) // SmallPlayer
        {
            GameObject[] finishes = null;

            // Wait until at least one Finish prefab exists in the scene
            while (finishes == null || finishes.Length == 0)
            {
                finishes = GameObject.FindGameObjectsWithTag("SPFinish");
                yield return null; // wait 1 frame
            }

            GameObject chosenFinish = finishes[Random.Range(0, finishes.Length)];
            Transform spStart = chosenFinish.transform.Find("SPStart");

            if (spStart == null)
            {
                Debug.LogError("SPStart not found in Finish prefab!");
                yield break;
            }

            PhotonNetwork.Instantiate(
                "SmallPlayer",
                spStart.position,
                spStart.rotation
            );
        }
        else // BigPlayer
        {
            PhotonNetwork.Instantiate(
                "BigPlayer",
                bigPlayerSpawnPoint.position,
                Quaternion.identity
            );
        }
    }

    // ---------- Player Left Room ----------
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (isLeaving) return;
        isLeaving = true;

        Debug.Log($"{otherPlayer.NickName} left the room. Disconnecting all players.");
        StartCoroutine(LeaveRoomAndLoadMenu());
    }

    // ---------- Photon-safe leave ----------
    private IEnumerator LeaveRoomAndLoadMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // async → OnLeftRoom will be called
        }
        else if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect(); // async → OnDisconnected will be called
        }
        else
        {
            LoadMainMenu(); // already disconnected
        }

        yield return null;
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        else
            LoadMainMenu();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (joinButtonText != null)
            joinButtonText.text = "Join";

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}


