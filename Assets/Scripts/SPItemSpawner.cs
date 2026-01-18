using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class SPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;
    public Transform[] spawnPoints;

    private List<GameObject> spawnList = new List<GameObject>();
    private bool itemsSpawned = false;

    void Awake()
    {
        // Auto-assign spawnPoints if none set in Inspector
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = GetComponentsInChildren<Transform>(true)
                .Where(t => t != transform) // ignore root
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
        yield return new WaitForSeconds(0.1f); // allow scene to initialize
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
            Debug.LogWarning("[SPItemSpawner] No spawn points found!");
            return;
        }

        Debug.Log("[SPItemSpawner] Spawning items. SpawnPoints: " + spawnPoints.Length);

        CreateSpawnList();

        for (int i = 0; i < spawnPoints.Length && i < spawnList.Count; i++)
        {
            GameObject prefab = spawnList[i];
            Transform spawnPoint = spawnPoints[i];

            PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
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


