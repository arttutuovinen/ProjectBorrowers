using UnityEngine;
using System.Collections.Generic;

public class SPItemSpawner : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;      // Prefabs to spawn
    public Transform[] spawnPoints;       // Empty objects as spawn points

    private List<GameObject> spawnList = new List<GameObject>();

    void Start()
    {
        CreateSpawnList();
        SpawnItems();
    }

    void CreateSpawnList()
    {
        spawnList.Clear();

        // Keep filling until we have enough items for all spawn points
        while (spawnList.Count < spawnPoints.Length)
        {
            // Shuffle prefabs each cycle
            List<GameObject> shuffled = new List<GameObject>(itemPrefabs);
            Shuffle(shuffled);

            // Add shuffled prefabs into the main spawn list
            spawnList.AddRange(shuffled);
        }

        // Trim to exact spawn point count
        if (spawnList.Count > spawnPoints.Length)
        {
            spawnList.RemoveRange(spawnPoints.Length, spawnList.Count - spawnPoints.Length);
        }
    }

    void SpawnItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(spawnList[i], spawnPoints[i].position, spawnPoints[i].rotation);
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
