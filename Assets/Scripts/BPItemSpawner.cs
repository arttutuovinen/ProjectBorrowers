using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class BPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;      // Prefabs to spawn
    public Transform[] spawnPoints;       // Empty objects as spawn points

    [Header("Spawn Chances (%) - Match Prefabs Order")]
    [Range(0, 100)]
    public int[] spawnChances = { 35, 35, 20, 10 };

    private List<GameObject> spawnList = new List<GameObject>();
    private bool itemsSpawned = false; // Track if items have been spawned

    // Called when this client joins the room
    public override void OnJoinedRoom()
    {
        // Request the MasterClient to spawn items
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RequestItemSpawnRPC), RpcTarget.MasterClient);
        }
        else
        {
            // If we are MasterClient, spawn items immediately
            SpawnItemsMaster();
        }
    }

    // Called when a new player joins
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Only MasterClient should spawn items
        if (!PhotonNetwork.IsMasterClient) return;

        // Spawn BigPlayer items for all players if not already spawned
        if (!itemsSpawned)
            SpawnItemsMaster();
    }

    [PunRPC]
    private void RequestItemSpawnRPC()
    {
        // Only MasterClient responds
        if (!PhotonNetwork.IsMasterClient) return;

        // Spawn items for everyone
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

            PhotonNetwork.InstantiateRoomObject(
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

        int totalSpawnPoints = spawnPoints.Length;

        // Calculate total chance
        int totalChance = 0;
        foreach (int c in spawnChances) totalChance += c;

        // Step 1: assign counts per prefab
        int[] prefabCounts = new int[itemPrefabs.Length];
        int assigned = 0;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            prefabCounts[i] = Mathf.FloorToInt((spawnChances[i] / (float)totalChance) * totalSpawnPoints);
            assigned += prefabCounts[i];
        }

        // Step 2: distribute leftover slots
        int remaining = totalSpawnPoints - assigned;
        while (remaining > 0)
        {
            for (int i = 0; i < prefabCounts.Length && remaining > 0; i++)
            {
                prefabCounts[i]++;
                remaining--;
            }
        }

        // Step 3: build spawnList
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            for (int j = 0; j < prefabCounts[i]; j++)
            {
                spawnList.Add(itemPrefabs[i]);
            }
        }

        // Step 4: shuffle
        Shuffle(spawnList);
    }

    // Fisher–Yates shuffle
    private void Shuffle<T>(List<T> list)
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


