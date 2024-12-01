using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SPIsStunned : MonoBehaviour
{
    public float stunDuration = 5f;    // Duration for which the player is stunned
    private bool isStunned = false;    // Tracks if the player is stunned
    public SmallPlayerMovement smallPlayerMovement; // Reference to the player's movement script (assuming a separate movement script exists)
    public TextMeshProUGUI spIsStunnedText;  // Reference to the TextMeshPro UI element

    void Start()
    {
        // Assuming the player has a movement script called "PlayerMovement"
        smallPlayerMovement = GetComponent<SmallPlayerMovement>();
        spIsStunnedText.enabled = false;
    }

    // Detect when the player touches an item
    private void OnTriggerEnter(Collider other)
    {
        // Check if the item is tagged as "StunItem"
        if (other.gameObject.CompareTag("MouseTrapWeapon") && !isStunned)
        {
            // Start the stun process
            StartCoroutine(StunPlayer());
            Debug.Log("Hits boppyPin");
            // Destroy the StunItem after collision
            Destroy(other.gameObject);
        }
    }

    // Coroutine to handle the stun logic
    IEnumerator StunPlayer()
    {
        // Set the player to stunned
        isStunned = true;
        spIsStunnedText.enabled = true;

        // Disable the player's movement
        if (smallPlayerMovement != null)
        {
            smallPlayerMovement.enabled = false;
        }

        // Wait for the stun duration
        yield return new WaitForSeconds(stunDuration);

        // Re-enable movement after stun is over
        if (smallPlayerMovement != null)
        {
            smallPlayerMovement.enabled = true;
        }
        spIsStunnedText.enabled = false;
        // Reset the stun state
        isStunned = false;
    }
}
