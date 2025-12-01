using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class SPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;
    public Transform[] spawnPoints;

    private List<GameObject> spawnList = new List<GameObject>();
    private bool itemsSpawned = false;

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Ask MasterClient to spawn items
            photonView.RPC(nameof(RequestItemSpawnRPC), RpcTarget.MasterClient);
        }
        else
        {
            SpawnItemsMaster();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Ensure items exist for late joiners
        if (!itemsSpawned)
        {
            SpawnItemsMaster();
        }
    }

    [PunRPC]
    private void RequestItemSpawnRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SpawnItemsMaster();
    }

    private void SpawnItemsMaster()
    {
        if (itemsSpawned) return;

        CreateSpawnList();

        for (int i = 0; i < spawnPoints.Length && i < spawnList.Count; i++)
        {
            GameObject prefab = spawnList[i];
            Transform spawnPoint = spawnPoints[i];

            PhotonNetwork.Instantiate(
                prefab.name,
                spawnPoint.position,
                spawnPoint.rotation
            );
        }

        itemsSpawned = true;
    }

    private void CreateSpawnList()
    {
        spawnList.Clear();

        while (spawnList.Count < spawnPoints.Length)
        {
            List<GameObject> shuffled = new List<GameObject>(itemPrefabs);
            Shuffle(shuffled);
            spawnList.AddRange(shuffled);
        }

        if (spawnList.Count > spawnPoints.Length)
        {
            spawnList.RemoveRange(spawnPoints.Length, spawnList.Count - spawnPoints.Length);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}


