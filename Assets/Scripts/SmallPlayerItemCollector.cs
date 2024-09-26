using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SmallPlayerItemCollector : MonoBehaviour
{
    public GameObject itemPrefab;    // The prefab of the item to spawn
    private bool hasItem = false;    // Tracks if the player has collected an item
    private bool itemSpawned = false; // Tracks if the item has been spawned after collection
    private GameObject collectedItem; // The reference to the collected item prefab
    private bool canPickUp = false;
    public TextMeshProUGUI pickUpText;  // Reference to the TextMeshPro UI element
    public Image boppyPinItemImage;  // Reference to the TextMeshPro UI element

    void Start()
    {
        // Disable the pickup text at the start of the game
        pickUpText.enabled = false;
        boppyPinItemImage.enabled = false;
    }
    // Detect when the player enters the collider of a collectible item
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player is near a collectible item
        if (other.gameObject.CompareTag("Collectible") && !hasItem)
        {
            // Store the reference to the collectible item the player can pick up
            collectedItem = other.gameObject;
            canPickUp = true;  // Player can now pick up the item

            // Enable the TextMeshPro text to show the pickup message
            pickUpText.enabled = true;
        }
    }
    // Detect when the player leaves the collider of the collectible item
    private void OnTriggerExit(Collider other)
    {
        // If the player moves away from the collectible item, they can no longer pick it up
        if (other.gameObject.CompareTag("Collectible"))
        {
            canPickUp = false;
            collectedItem = null; // Reset the collectible reference
            pickUpText.enabled = false;
        }
    }


    void Update()
    {
        // Check if the player is in range to pick up the item and presses "P1Interact"
        if (canPickUp && !hasItem && Input.GetButtonDown("P1Interact"))
        {
            // Collect the item by destroying it and marking it as collected
            Destroy(collectedItem);
            hasItem = true;
            itemSpawned = false; // Reset the spawned status
            collectedItem = null; // Clear the reference to the collected item
            canPickUp = false;  // No longer in range since item is collected

            // Disable the TextMeshPro text after picking up the item
            pickUpText.enabled = false;
            boppyPinItemImage.enabled = true;
        }

        // Check if the player presses "E", has collected an item, and hasn't spawned it yet
        if (hasItem && !itemSpawned && Input.GetButtonDown("P1UseItem"))
        {
            // Spawn the collected item at the player's position
            Instantiate(itemPrefab, transform.position + transform.forward, Quaternion.identity);

            // Mark the item as spawned so it can't be spawned again
            itemSpawned = true;

            // The player no longer has the item, it has been used/spawned
            hasItem = false;
            boppyPinItemImage.enabled = false;
        }
    }
}
