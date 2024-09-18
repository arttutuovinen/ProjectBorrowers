using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Door : MonoBehaviour
{
    // How much the door should rotate on the Y-axis
    public float rotationAmount = 55.0f;

    // Flag to check if the player is within the trigger area
    private bool playerInTrigger = false;

    // Flag to check if the door is currently open or closed
    private bool isDoorOpen = false;
    // The door's initial rotation
    private Quaternion initialRotation;

    public TextMeshProUGUI interactText;

    void Start()
    {
        // Store the initial rotation of the door to apply the rotation relative to it
        initialRotation = transform.rotation;
        
        // Hide the interact text at the start
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false); // Disable the text object initially
        }

    }

    void Update()
    {
        // If the player is in the trigger area and presses the pick up button
        if (playerInTrigger && Input.GetButtonDown("P2PickUp"))
        {
            // Toggle the door rotation
            if (isDoorOpen)
            {
                RotateDoorBack();
            }
            else
            {
                RotateDoorOpen();
            }

            // Toggle the door state
            isDoorOpen = !isDoorOpen;
        }
    }

    // Method to rotate the door by a certain amount
    void RotateDoorOpen()
    {
        // Create the rotation quaternion for 55 degrees around the Y-axis
        Quaternion targetRotation = Quaternion.Euler(0, rotationAmount, 0);

        // Apply the rotation relative to the door's initial rotation
        transform.rotation = initialRotation * targetRotation;
    }
    // Method to rotate the door back to its initial position
    void RotateDoorBack()
    {
        // Reset the rotation to the initial rotation
        transform.rotation = initialRotation;
    }

    // Trigger detection for when the player enters the door's trigger collider
    void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding with the door is the player (you can change the tag name if needed)
        if (other.CompareTag("BigPlayer"))
        {
            playerInTrigger = true;
            // Enable the interact text when the player enters the trigger
            if (interactText != null)
            {
                interactText.gameObject.SetActive(true); // Activate text when the player enters the trigger
            }
                
        }
    }

    // Trigger detection for when the player exits the door's trigger collider
    void OnTriggerExit(Collider other)
    {
        // Check if the object exiting the trigger is the player
        if (other.CompareTag("BigPlayer"))
        {
            playerInTrigger = false;
            // Deactivate the interact text when the player leaves the trigger
            if (interactText != null)
            {
                interactText.gameObject.SetActive(false); // Deactivate text when the player exits the trigger
            }
        }
    }
}
