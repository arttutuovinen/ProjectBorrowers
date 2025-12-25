using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using System.Collections;

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
            // Spawn 5 Finish prefabs
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        if (joinButtonText != null)
            joinButtonText.text = "Join";

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }
}


