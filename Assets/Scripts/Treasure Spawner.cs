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
    private CompassBar compassBar;            // Reference to the CompassBar script

    void Start()
    {
        compassBar = FindObjectOfType<CompassBar>();
        spawnPoints = GameObject.FindGameObjectsWithTag("TreasureSpawnPoint");
        smallPlayerScoreText.text = score.ToString();
        TeleportTreasureToRandomPoint();
    }

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
            TeleportTreasureToRandomPoint();
            compassBar.ResetTreasure();
        }
        
    }
    
}
