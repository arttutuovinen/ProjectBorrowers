using UnityEngine;
using System.Collections.Generic;
public class BPItemSpawner : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject[] itemPrefabs;      // Prefabs to spawn
    public Transform[] spawnPoints;       // Empty objects as spawn points

    [Header("Spawn Chances (%) - Match Prefabs Order")]
    [Range(0, 100)]
    public int[] spawnChances = { 35, 35, 20, 10 };

    private List<GameObject> spawnList = new List<GameObject>();

    void Start()
    {
        CreateSpawnList();
        SpawnItems();
    }

    void CreateSpawnList()
    {
        spawnList.Clear();

        int totalSpawnPoints = spawnPoints.Length;

        // Calculate total of all chances (doesn't have to be 100)
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

        // Step 2: distribute any leftover slots (because of rounding)
        int remaining = totalSpawnPoints - assigned;
        while (remaining > 0)
        {
            for (int i = 0; i < prefabCounts.Length && remaining > 0; i++)
            {
                prefabCounts[i]++;
                remaining--;
            }
        }

        // Step 3: build the spawnList
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
