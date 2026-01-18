using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;

public class TreasureSpawner : MonoBehaviourPunCallbacks
{
    public GameObject treasure;
    private Transform[] spawnPoints;

    void Awake()
    {
        // Collect all spawn points that are children
        var points = GetComponentsInChildren<Transform>(true);
        spawnPoints = System.Array.FindAll(points, t => t.CompareTag("TreasureSpawnPoint"));
    }

    void Start()
    {
        // Ensure treasure is inactive until teleport
        if (treasure != null) treasure.SetActive(false);

        // Only MasterClient teleports the treasure
        if (PhotonNetwork.IsMasterClient)
        {
            // Delay to ensure all PhotonViews in scene are initialized
            StartCoroutine(TeleportAfterSceneReady());
        }
    }

    private IEnumerator TeleportAfterSceneReady()
    {
        // Wait a couple of frames to guarantee PhotonViews are ready
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        TeleportTreasure();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Ensure the new player sees treasure in the right location
            TeleportTreasure();
        }
    }

    // PUBLIC: can be called from FinishTrigger
    public void TeleportTreasure()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (spawnPoints.Length == 0 || treasure == null) return;

        // Pick a random spawn point
        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        // Move treasure locally
        treasure.transform.SetPositionAndRotation(point.position, point.rotation);
        treasure.SetActive(true);

        // Broadcast to all clients, including late joiners
        PhotonView pv = treasure.GetComponent<PhotonView>();
        if (pv != null)
        {
            pv.RPC("RPC_TeleportTreasure", RpcTarget.AllBuffered, point.position, point.rotation);
        }
    }
}


