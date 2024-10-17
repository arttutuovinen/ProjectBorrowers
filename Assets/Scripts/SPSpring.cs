using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPSpring : MonoBehaviour
{
    public float knockUpForce = 10f; // Force applied when the player is knocked up
    public float knockUpDuration = 0.5f; // Duration of the knock-up effect
    private SmallPlayerMovement playerMovement;
    private bool isKnockedUp = false; // Flag to check if the player is currently knocked up
    private float knockUpTimer = 0f;

    private void Start()
    {
        // Get the reference to the player's SmallPlayerMovement script
        playerMovement = GetComponent<SmallPlayerMovement>();
    }

    private void Update()
    {
        // Handle knock-up effect duration
        if (isKnockedUp)
        {
            knockUpTimer -= Time.deltaTime;

            // Apply the upward force to the player
            if (knockUpTimer > 0)
            {
                Vector3 knockUpVelocity = Vector3.up * knockUpForce * Time.deltaTime;
            }
            else
            {
                // End the knock-up effect
                isKnockedUp = false;
            }
        }
    }

    // Method to initiate the knock-up effect
    public void KnockUpPlayer()
    {
        if (!isKnockedUp)
        {
            // Start the knock-up
            isKnockedUp = true;
            knockUpTimer = knockUpDuration;
        }
    }
}
