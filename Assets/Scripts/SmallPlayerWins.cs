using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerWins : MonoBehaviour
{
    // Reference to the player object
    public GameObject smallPlayer;

    

    // Tag for the treasure GameObject
    public string treasureTag = "Treasure";

    // Tag for the player GameObject
    public string playerTag = "SmallPlayer";

    // Called when the CharacterController collides with another object
    private void OnTriggerEnter(Collider hit)
    {
        // Check if the player has collided with the object that destroys them and if they've collected the treasure
        if (hit.gameObject.CompareTag(playerTag) && hit.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected == true)
        {
            Destroy(smallPlayer);  // Destroy the player GameObject
            Debug.Log("Player destroyed!");
        }
    }

    

    private void Start()
    {
        // Simple debugging to ensure the player object is assigned correctly
        if (smallPlayer == null)
        {
            Debug.LogError("Player object reference is missing!");
        }
    }
}
