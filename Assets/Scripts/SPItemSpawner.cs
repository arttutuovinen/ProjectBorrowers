using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SPItemSpawner : MonoBehaviourPunCallbacks
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;
    public Transform[] spawnPoints;

    private List<GameObject> spawnList = new List<GameObject>();

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CreateSpawnList();
            SpawnItems();
        }
    }

    void CreateSpawnList()
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

    void SpawnItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            PhotonNetwork.Instantiate(
                spawnList[i].name,
                spawnPoints[i].position,
                spawnPoints[i].rotation
            );
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}

