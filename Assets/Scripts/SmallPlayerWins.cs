using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerWins : MonoBehaviour
{
    public GameObject smallPlayer;
    public string treasureTag = "Treasure";
    public string playerTag = "SmallPlayer";

    // Called when the CharacterController collides with another object
    private void OnTriggerEnter(Collider hit)
    {
        // Check if the player has collided with the object that destroys them and if they've collected the treasure
        if (hit.gameObject.CompareTag(playerTag) && hit.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected == true)
        {
            Destroy(smallPlayer);  // Destroy the player GameObject
            Debug.Log("Small Player Wins!");
        }
    }
}
