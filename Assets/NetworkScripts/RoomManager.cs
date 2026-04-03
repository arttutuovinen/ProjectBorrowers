using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Scene")]
    public Transform[] spFinishSpawnPoints;
    public Transform bigPlayerSpawnPoint;
    public Transform[] prisonSpawnPoints;

    private bool isLeaving = false;

    void Start()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // Spawn shared objects
            SpawnFinishes();
            SpawnPrison();

            // Spawn items first
            StartCoroutine(SpawnItemsThenPlayers());
        }
        else
        {
            // Non-Master clients: just spawn players after a short delay
            StartCoroutine(DelayedPlayerSpawn());
        }
    }
    private IEnumerator SpawnItemsThenPlayers()
    {
        // Wait a frame to ensure scene objects exist
        yield return null;

        // Find all item spawners and tell them to spawn items
        foreach (var spawner in FindObjectsOfType<SPItemSpawner>())
            spawner.SpawnItemsMaster(); // make SpawnItemsMaster public

        foreach (var spawner in FindObjectsOfType<BPItemSpawner>())
            spawner.SpawnItemsMaster(); // make SpawnItemsMaster public

        // Wait one frame to ensure all items instantiated
        yield return null;

        // Now spawn players
        StartCoroutine(SpawnPlayerFromLobbyRole());
    }

    private IEnumerator DelayedPlayerSpawn()
    {
        // Wait for MasterClient to spawn items
        yield return new WaitForSeconds(0.2f);

        StartCoroutine(SpawnPlayerFromLobbyRole());
    }
    // ---------- Shared Objects ----------

    void SpawnFinishes()
    {
        var selectedPoints = spFinishSpawnPoints
            .OrderBy(x => Random.value)
            .Take(RequiredPlayeramount.SPFinishCount);

        foreach (var point in selectedPoints)
        {
            PhotonNetwork.Instantiate(
                "SPFinish",
                point.position,
                point.rotation
            );
        }
    }

    void SpawnPrison()
    {
        if (prisonSpawnPoints == null || prisonSpawnPoints.Length == 0)
            return;

        int rand = Random.Range(0, prisonSpawnPoints.Length);
        PhotonNetwork.Instantiate(
            "Prison",
            prisonSpawnPoints[rand].position,
            Quaternion.identity
        );
    }

    // ---------- Player Spawn ----------

    IEnumerator SpawnPlayerFromLobbyRole()
    {
        // Wait until role is available
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(LobbyKeys.PlayerRole))
            yield return null;

        int role = (int)PhotonNetwork.LocalPlayer.CustomProperties[LobbyKeys.PlayerRole];

        if (role == 1) // SmallPlayer
        {
            GameObject[] finishes = null;

            while (finishes == null || finishes.Length == 0)
            {
                finishes = GameObject.FindGameObjectsWithTag("SPFinish");
                yield return null;
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
            GameObject spawnObj = null;

            while (spawnObj == null)
            {
                spawnObj = GameObject.FindGameObjectWithTag("BigSpawn");
                yield return null;
            }

            PhotonNetwork.Instantiate(
                "BigPlayer",
                spawnObj.transform.position,
                spawnObj.transform.rotation
            );
        }


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- Player Leave Handling ----------

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isLeaving) return;
        isLeaving = true;

        Debug.Log($"{otherPlayer.NickName} left. Returning to Main Menu.");
        StartCoroutine(LeaveRoomAndLoadMenu());
    }

    IEnumerator LeaveRoomAndLoadMenu()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        yield return null;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }
}