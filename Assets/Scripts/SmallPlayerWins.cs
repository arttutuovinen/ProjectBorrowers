using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SmallPlayerWins : MonoBehaviour
{
    public GameObject smallPlayer;
    public string treasureTag = "Treasure";
    public string playerTag = "SmallPlayer";
    public TextMeshProUGUI smallPlayerWinsText;
    public float resetDelay = 5f;

    void Start()
    {
        smallPlayerWinsText.enabled = false;
    }

    // Called when the CharacterController collides with another object
    private void OnTriggerEnter(Collider hit)
    {
        // Check if the player has collided with the object that destroys them and if they've collected the treasure
        if (hit.gameObject.CompareTag(playerTag) && hit.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected == true)
        {
            Destroy(smallPlayer);  // Destroy the player GameObject
            Debug.Log("Small Player Wins!");
            smallPlayerWinsText.enabled = true;
            StartCoroutine(RestartSceneAfterDelay());
        }
    }
    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay); // Wait for the specified delay
        Scene currentScene = SceneManager.GetActiveScene(); // Get the current active scene
        SceneManager.LoadScene(currentScene.name); // Reload the scene
    }
}
