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

    // Called when this client joins a room
    public override void OnJoinedRoom()
    {
        // Ask the MasterClient to spawn items
        photonView.RPC(nameof(RequestItemSpawnRPC), RpcTarget.MasterClient);
    }

    // Called when any new player joins the room (only fires on MasterClient)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // If items are not already spawned, ensure they spawn for all clients
        if (spawnList.Count == 0)
        {
            CreateSpawnList();
            SpawnItems();
        }
    }

    [PunRPC]
    private void RequestItemSpawnRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CreateSpawnList();
        SpawnItems();
    }

    private void CreateSpawnList()
    {
        spawnList.Clear();

        int totalSpawnPoints = spawnPoints.Length;

        // Calculate total of all chances
        int totalChance = 0;
        foreach (int c in spawnChances) totalChance += c;

        // Step 1: assign count per prefab based on percentage
        int[] prefabCounts = new int[itemPrefabs.Length];
        int assigned = 0;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            prefabCounts[i] = Mathf.FloorToInt((spawnChances[i] / (float)totalChance) * totalSpawnPoints);
            assigned += prefabCounts[i];
        }

        // Step 2: distribute any leftover slots
        int remaining = totalSpawnPoints - assigned;
        while (remaining > 0)
        {
            for (int i = 0; i < prefabCounts.Length && remaining > 0; i++)
            {
                prefabCounts[i]++;
                remaining--;
            }
        }

        // Step 3: build the spawn list
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            for (int j = 0; j < prefabCounts[i]; j++)
            {
                spawnList.Add(itemPrefabs[i]);
            }
        }

        // Step 4: shuffle so spawns are randomized
        Shuffle(spawnList);
    }

    private void SpawnItems()
    {
        for (int i = 0; i < spawnPoints.Length && i < spawnList.Count; i++)
        {
            PhotonNetwork.InstantiateRoomObject(
                spawnList[i].name,
                spawnPoints[i].position,
                spawnPoints[i].rotation
            );
        }
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

