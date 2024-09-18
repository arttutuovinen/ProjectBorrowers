using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerSpawner : MonoBehaviour
{
    // Array to hold all spawn points found in the scene
    private GameObject[] spawnPoints;

    // Reference to the player object in the scene
    public GameObject player;

    // Reference to the "Finish" object in the scene
    public GameObject finish;

    void Start()
    {
        // Find all spawn points in the scene tagged as "SpawnPoint"
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Check if there are any spawn points
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found in the scene!");
            return;
        }

        // Teleport the player to a random spawn point
        TeleportPlayerToRandomPoint();
    }

    // Method to teleport the player to a random spawn point
    void TeleportPlayerToRandomPoint()
    {
        if (player == null)
        {
            Debug.LogError("Player object not set!");
            return;
        }
        if (finish == null)
        {
            Debug.LogError("Finish object not set!");
            return;
        }


        // Choose a random spawn point from the array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject randomSpawnPoint = spawnPoints[randomIndex];

        // Move the player to the chosen spawn point's position and rotation
        player.transform.position = randomSpawnPoint.transform.position;
        player.transform.rotation = randomSpawnPoint.transform.rotation;

        // Move the "Finish" object to the same position and rotation as the player
        finish.transform.position = player.transform.position;
        finish.transform.rotation = player.transform.rotation;
    }
}
