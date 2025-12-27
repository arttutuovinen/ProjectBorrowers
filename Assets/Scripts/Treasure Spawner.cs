using UnityEngine;
using Photon.Pun;
using System.Collections;

public class TreasureSpawner : MonoBehaviourPunCallbacks
{
    public GameObject treasure;
    private Transform[] spawnPoints;

    void Start()
    {
        // Collect all spawn points that are children and active
        var points = GetComponentsInChildren<Transform>(true);
        spawnPoints = System.Array.FindAll(points, t => t.CompareTag("TreasureSpawnPoint"));
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(TeleportAfterReady());
    }

    private IEnumerator TeleportAfterReady()
    {
        // Wait until spawn points exist and treasure is assigned
        yield return new WaitUntil(() => spawnPoints != null && spawnPoints.Length > 0 && treasure != null);
        TeleportTreasure();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            TeleportTreasure();
    }

    public void TeleportTreasure()
    {
        if (!PhotonNetwork.IsMasterClient || spawnPoints.Length == 0 || treasure == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        PhotonView pv = treasure.GetComponent<PhotonView>();
        if (pv != null)
            pv.RPC("RPC_TeleportTreasure", RpcTarget.AllBuffered, point.position, point.rotation);
    }


    [PunRPC]
    public void RPC_TeleportTreasure(Vector3 pos, Quaternion rot)
    {
        if (treasure != null)
        {
            treasure.transform.SetPositionAndRotation(pos, rot);
            treasure.SetActive(true);
        }
    }
}


