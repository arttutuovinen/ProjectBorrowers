using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    // Array to hold all spawn points found in the scene
    private GameObject[] spawnPoints;

    // Reference to the player object in the scene
    public GameObject treasure;

    void Start()
    {
        // Find all spawn points in the scene tagged as "SpawnPoint"
        spawnPoints = GameObject.FindGameObjectsWithTag("TreasureSpawnPoint");

        // Check if there are any spawn points
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found in the scene!");
            return;
        }

        // Teleport the player to a random spawn point
        TeleportTreasureToRandomPoint();
    }

    // Method to teleport the player to a random spawn point
    void TeleportTreasureToRandomPoint()
    {
        if (treasure == null)
        {
            Debug.LogError("Treasure object not set!");
            return;
        }

        // Choose a random spawn point from the array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject randomSpawnPoint = spawnPoints[randomIndex];

        // Move the player to the chosen spawn point's position and rotation
        treasure.transform.position = randomSpawnPoint.transform.position;
        treasure.transform.rotation = randomSpawnPoint.transform.rotation;
    }
}
