using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPCatchingSP : MonoBehaviour
{
    public string bigPlayerColliderTag = "BPHand";  // Tag for Big Player's collider
    public string smallPlayerTag = "SmallPlayer";  // Tag for Small Player
    public float rayDistance = 10f;  // Distance to check with the raycast
    public LayerMask blockingMask;    // LayerMask for walls/obstacles
    public Color rayColor = Color.red;  // Color of the ray in the Scene view

    public Camera playerCamera;  // Reference to the Big Player's camera (assign via Inspector)
    public GameObject smallPlayer;  // Publicly editable reference to the Small Player
    public Transform teleportLocation;  // Publicly editable location to teleport the Small Player


    // This method is called when the Small Player's trigger collider enters the Big Player's collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that triggered the event has the Small Player tag
        if (other.CompareTag(smallPlayerTag))
        {
            // Check if this Big Player's collider has the required tag
            if (this.CompareTag(bigPlayerColliderTag))
            {
                // Cast a ray from the camera position to the forward direction of the camera
                Vector3 rayOrigin = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward;

                // Perform the raycast from the camera's view
                if (!Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, blockingMask))
                {
                    // No blocking object found, teleport the Small Player to the specified location
                    Debug.Log("Small Player's trigger collided with Big Player's tagged collider. Teleporting Small Player.");
                    TeleportSmallPlayer();
                }
                else
                {
                    Debug.Log("A blocking object is between the Big Player (camera view) and Small Player: " + hit.collider.name);
                }

                // Draw the ray in the Scene view for visualization
                Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 2.0f);
            }
            else
            {
                Debug.Log("Big Player's collider doesn't have the required tag.");
            }
        }
    }

    // Method to teleport the Small Player to the assigned teleport location
    private void TeleportSmallPlayer()
    {
        if (smallPlayer != null && teleportLocation != null)
        {
            smallPlayer.transform.position = teleportLocation.position;
            Debug.Log("Small Player has been teleported to: " + teleportLocation.position);
        }
        else
        {
            Debug.LogWarning("Small Player or Teleport location not set. Please assign them in the Inspector.");
        }
    }
}
