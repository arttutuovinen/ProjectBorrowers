using UnityEngine;
using Photon.Pun;

public class TreasureSpawner : MonoBehaviourPunCallbacks
{
    public GameObject treasure;
    private GameObject[] spawnPoints;

    void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("TreasureSpawnPoint");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            TeleportTreasure();
    }

    public void TeleportTreasure()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (treasure == null || spawnPoints.Length == 0) return;

        int index = Random.Range(0, spawnPoints.Length);
        GameObject point = spawnPoints[index];

        PhotonView pv = treasure.GetComponent<PhotonView>();
        pv.RPC("RPC_TeleportTreasure", RpcTarget.AllBuffered,
            point.transform.position,
            point.transform.rotation);
    }
    [PunRPC]
    public void RPC_TeleportTreasure(Vector3 pos, Quaternion rot)
    {
        if (treasure != null)
        {
            treasure.transform.SetPositionAndRotation(pos, rot);
            treasure.SetActive(true); // Reactivate treasure on all clients
        }
    }
}


