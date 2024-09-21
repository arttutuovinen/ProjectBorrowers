using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Door : MonoBehaviour
{
    public float rotationAmount = 80.0f;  // How much the door should rotate
    public float rotationSpeed = 5.0f;    // Speed of the door opening and closing
    private bool isDoorOpen = false;      // Check if the door is open
    private Quaternion initialRotation;   // Store the initial rotation of the door
    private Quaternion targetRotation;    // The target rotation to open/close the door

    private void Start()
    {
        // Store the initial rotation of the door
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        // Smoothly rotate the door to the target rotation over time
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Method to toggle the door's state (open/close)
    public void ToggleDoor()
    {
        if (isDoorOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }

        isDoorOpen = !isDoorOpen;  // Toggle the door state
    }

    // Open the door (rotate 55 degrees around the Y-axis)
    private void OpenDoor()
    {
        targetRotation = Quaternion.Euler(0, rotationAmount, 0) * initialRotation;
    }

    // Close the door (rotate back to the initial position)
    private void CloseDoor()
    {
        targetRotation = initialRotation;
    }
}
