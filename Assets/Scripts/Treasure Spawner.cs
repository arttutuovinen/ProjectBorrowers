using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class TreasureSpawner : MonoBehaviour
{
    private GameObject[] spawnPoints;
    public GameObject treasure;
    public string playerTag = "SmallPlayer";
    public TextMeshProUGUI smallPlayerScoreText;
    private int score = 0;

    void Start()
    {
        // Find all spawn points in the scene tagged as "SpawnPoint"
        spawnPoints = GameObject.FindGameObjectsWithTag("TreasureSpawnPoint");
        smallPlayerScoreText.text = score.ToString();
        // Teleport the player to a random spawn point
        TeleportTreasureToRandomPoint();
    }

    // Method to teleport the player to a random spawn point
    void TeleportTreasureToRandomPoint()
    {

        // Choose a random spawn point from the array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject randomSpawnPoint = spawnPoints[randomIndex];
        treasure.transform.position = randomSpawnPoint.transform.position;
        treasure.transform.rotation = randomSpawnPoint.transform.rotation;
        if (!treasure.activeSelf) // reactivate if it was turned off
        treasure.SetActive(true);
        
    }

    // Called when the CharacterController collides with another object
    private void OnTriggerEnter(Collider hit)
    {
        // Check if the player has collided with the object that destroys them and if they've collected the treasure
        if (hit.gameObject.CompareTag(playerTag) && hit.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected == true)
        {
            score++;
            smallPlayerScoreText.text = score.ToString();
            hit.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected = false;
            treasure.SetActive(false);
            TeleportTreasureToRandomPoint();
        }
    }
    
}
