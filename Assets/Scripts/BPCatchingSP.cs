using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BPCatchingSP : MonoBehaviour
{
    public string bigPlayerColliderTag = "BPHand";  // Tag for Big Player's collider
    public string smallPlayerTag = "SmallPlayer";  // Tag for Small Player
    public float rayDistance = 10f;  // Distance to check with the raycast
    public LayerMask blockingMask;    // LayerMask for walls/obstacles
    public Color rayColor = Color.red;  // Color of the ray in the Scene view

    public Camera playerCamera;  // Reference to the Big Player's camera (assign via Inspector)
    public GameObject smallPlayer;  // Publicly editable reference to the Small Player
    public Transform caughtLocation;  // Publicly editable location to teleport the Small Player
    public Transform jailLocation;  // Location to teleport the Small Player when sent t

    private bool isSmallPlayerAttachedToLocation = false;  // Flag to track if Player 2 should follow teleportLocation
    private bool isJailReady = false;  // Flag to check if Player 1's ray hit the "Jail"
    private bool isSmallPlayerCaught = false;  // Flag to track if the small player is caught

    public BigPlayerAnimation bigPlayerAnimation; // Reference to another script.

    public TextMeshProUGUI prisonInteractText;

    public SPEscapeBar spEscapeBar;
    private Collider objectCollider;

    private void Start()
    {
        prisonInteractText.gameObject.SetActive(false); // Disable the text object initially
        objectCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        // Cast a ray from Player 1's camera in the forward direction
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Check if the ray hits an object within the specified distance
        if (Physics.Raycast(ray, out hit, rayDistance, blockingMask))
        {
            // Check if the object hit is named "Prison"
            if (hit.collider.gameObject.name == "Prison")
            {
                isJailReady = true;
                Debug.Log("Player 1's ray hit the Jail object.");
            }
            if (isSmallPlayerCaught == true)
            {
                prisonInteractText.gameObject.SetActive(true); // Activate the text when the door is in range
            }
            else
            {
                isJailReady = false; // Reset if the ray hits something else
            }

            // Draw the ray in the Scene view for debugging
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);
        }
        else
        {
            isJailReady = false; // Reset if nothing is hit
            prisonInteractText.gameObject.SetActive(false);
        }

        // If Player 2 should stay attached to the teleport location, keep its position updated
        if (isSmallPlayerAttachedToLocation && smallPlayer != null && caughtLocation != null)
        {
            // Move Player 2 to the caught location position every frame to ensure they stay together
            smallPlayer.transform.position = caughtLocation.position;
        }

        // Check if the interaction button is pressed and the ray has hit the "Jail"
        if (isSmallPlayerCaught && isJailReady && Input.GetButtonDown("P2Interact"))
        {
            TeleportSmallPlayerToJail();
            bigPlayerAnimation.ReleaseAnimation();
        }

        // Check if meter value reaches 1 in spEscapeBar script.
        if (spEscapeBar.currentFill >= 0.98 && isSmallPlayerCaught)
        {
            bigPlayerAnimation.ReleaseAnimation();
            spEscapeBar.currentFill = 0f;
            // Re-enable Small Player's CharacterController
            CharacterController characterController = smallPlayer.GetComponent<CharacterController>();
            characterController.enabled = true;

            // Ensure Small Player's position is no longer equal to caughtLocation
            if (smallPlayer.transform.position == caughtLocation.position)
            {
                smallPlayer.transform.position += Vector3.down * 0.2f; // Slightly adjust position
            }

            isSmallPlayerAttachedToLocation = false;
            isSmallPlayerCaught = false;
             StartCoroutine(DisableColliderForOneSecond());
        }
        
    }

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
                    bigPlayerAnimation.CaughtAnimation();
                    isSmallPlayerCaught = true;  // Set the caught flag to true
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

    // Method to teleport Player 2 to the assigned caught location
    private void TeleportSmallPlayer()
    {
            // Get the CharacterController from Small Player
            CharacterController characterController = smallPlayer.GetComponent<CharacterController>();

            if (characterController != null)
            {
                // Temporarily disable the CharacterController before setting the position to avoid physics issues
                characterController.enabled = false;

                // Set Player 2's position to the caught location's position
                characterController.transform.position = caughtLocation.position;

                // Set the flag to keep Player 2 at the caught location
                isSmallPlayerAttachedToLocation = true;

                Debug.Log("Small Player has been teleported to: " + caughtLocation.position);
            }
        
    }

    // Method to teleport Player 2 to the "JailLocation"
    private void TeleportSmallPlayerToJail()
    {
        // Check if smallPlayer and jailLocation are assigned
        if (smallPlayer != null && jailLocation != null)
        {
            // Get the CharacterController from Small Player
            CharacterController characterController = smallPlayer.GetComponent<CharacterController>();

            if (characterController != null)
            {
                // Temporarily disable the CharacterController before setting the position to avoid physics issues
                characterController.enabled = false;

                // Set Player 2's position to the jail location's position
                characterController.transform.position = jailLocation.position;

                // Re-enable the CharacterController after teleporting
                characterController.enabled = true;

                // Reset the attachment to the caught location and caught flag
                isSmallPlayerAttachedToLocation = false;
                isSmallPlayerCaught = false;

                Debug.Log("Player 2 has been teleported to the Jail at: " + jailLocation.position);
            }
            else
            {
                Debug.LogWarning("CharacterController not found on Small Player.");
            }
        }
        else
        {
            Debug.LogWarning("Small Player or JailLocation is not assigned.");
        }
    }
    private IEnumerator DisableColliderForOneSecond()
    {
        
        Debug.Log("Disabling collider for 1 second.");
        objectCollider.enabled = false; // Disable the collider

        yield return new WaitForSeconds(1f); // Wait for 1 second

        objectCollider.enabled = true; // Re-enable the collider
    }
    
}
