using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlayerStun : MonoBehaviour
{
    public float stunDuration = 5f;    // Duration for which the player is stunned
    private bool isStunned = false;    // Tracks if the player is stunned
    public BigPlayerMovement bigPlayerMovement; // Reference to the player's movement script (assuming a separate movement script exists)

    void Start()
    {
        // Assuming the player has a movement script called "PlayerMovement"
        bigPlayerMovement = GetComponent<BigPlayerMovement>();
    }

    // Detect when the player touches an item
    private void OnTriggerEnter(Collider other)
    {
        // Check if the item is tagged as "StunItem"
        if (other.gameObject.CompareTag("StunItem") && !isStunned)
        {
            // Start the stun process
            StartCoroutine(StunPlayer());
            Debug.Log("Hits boppyPin");
        }
    }

    // Coroutine to handle the stun logic
    IEnumerator StunPlayer()
    {
        // Set the player to stunned
        isStunned = true;

        // Disable the player's movement
        if (bigPlayerMovement != null)
        {
            bigPlayerMovement.enabled = false;
        }

        // Wait for the stun duration
        yield return new WaitForSeconds(stunDuration);

        // Re-enable movement after stun is over
        if (bigPlayerMovement != null)
        {
            bigPlayerMovement.enabled = true;
        }

        // Reset the stun state
        isStunned = false;
    }
}
