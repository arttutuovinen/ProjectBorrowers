using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class BPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;      // Prefabs to spawn
    public Transform[] spawnPoints;       // Empty objects as spawn points

    [Header("Spawn Chances (%) - Match Prefabs Order")]
    [Range(0, 100)]
    public int[] spawnChances = { 12, 9, 9, 9, 9, 18, 9, 16, 9 };

    private List<GameObject> spawnList = new List<GameObject>();
    private bool itemsSpawned = false; // Track if items have been spawned

    void Awake()
    {
        // Auto-assign spawnPoints if none set
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = GetComponentsInChildren<Transform>(true)
                .Where(t => t != transform)
                .ToArray();
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnAfterDelay());
        }
        else
        {
            StartCoroutine(RequestSpawnAfterDelay());
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!itemsSpawned)
        {
            StartCoroutine(SpawnAfterDelay());
        }
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        SpawnItemsMaster();
    }

    private IEnumerator RequestSpawnAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        photonView.RPC(nameof(RequestItemSpawnRPC), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RequestItemSpawnRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SpawnItemsMaster();
    }

    public void SpawnItemsMaster()
    {
        if (itemsSpawned) return;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[BPItemSpawner] No spawn points found!");
            return;
        }

        Debug.Log("[BPItemSpawner] Spawning items. SpawnPoints: " + spawnPoints.Length);

        CreateSpawnList();

        for (int i = 0; i < spawnPoints.Length && i < spawnList.Count; i++)
        {
            GameObject prefab = spawnList[i];
            Transform spawnPoint = spawnPoints[i];

            PhotonNetwork.InstantiateRoomObject(prefab.name, spawnPoint.position, spawnPoint.rotation);
        }

        itemsSpawned = true;
    }

    private void CreateSpawnList()
    {
        spawnList.Clear();
        
        foreach (var prefab in itemPrefabs)
        {
            spawnList.Add(prefab);
        }
        int totalSpawnPoints = spawnPoints.Length - itemPrefabs.Length;
        int totalChance = spawnChances.Sum();

        int[] prefabCounts = new int[itemPrefabs.Length];
        int assigned = 0;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            prefabCounts[i] = Mathf.Max(1, Mathf.FloorToInt((spawnChances[i] / (float)totalChance) * totalSpawnPoints));
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

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}


