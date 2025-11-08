using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class BPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;      // Prefabs to spawn
    public Transform[] spawnPoints;       // Empty objects as spawn points

    [Header("Spawn Chances (%) - Match Prefabs Order")]
    [Range(0, 100)]
    public int[] spawnChances = { 35, 35, 20, 10 };

    private List<GameObject> spawnList = new List<GameObject>();

    public override void OnJoinedRoom()
    {
        // Always request item spawn from MasterClient
        photonView.RPC(nameof(RequestItemSpawn), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RequestItemSpawn()
    {
        // Only MasterClient performs the actual spawning
        if (!PhotonNetwork.IsMasterClient) return;

        CreateSpawnList();
        SpawnItems();
    }

    void CreateSpawnList()
    {
        spawnList.Clear();

        int totalSpawnPoints = spawnPoints.Length;

        int totalChance = 0;
        foreach (int c in spawnChances) totalChance += c;

        int[] prefabCounts = new int[itemPrefabs.Length];
        int assigned = 0;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            prefabCounts[i] = Mathf.FloorToInt((spawnChances[i] / (float)totalChance) * totalSpawnPoints);
            assigned += prefabCounts[i];
        }

        int remaining = totalSpawnPoints - assigned;
        while (remaining > 0)
        {
            for (int i = 0; i < prefabCounts.Length && remaining > 0; i++)
            {
                prefabCounts[i]++;
                remaining--;
            }
        }

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            for (int j = 0; j < prefabCounts[i]; j++)
            {
                spawnList.Add(itemPrefabs[i]);
            }
        }

        Shuffle(spawnList);
    }

    void SpawnItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Network-wide spawn using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(spawnList[i].name, spawnPoints[i].position, spawnPoints[i].rotation);
        }
    }

    // Fisher–Yates shuffle
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}

