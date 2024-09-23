using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerItemCollector : MonoBehaviour
{
    public GameObject itemPrefab;    // The prefab of the item to spawn
    private bool hasItem = false;    // Tracks if the player has collected an item
    private bool itemSpawned = false; // Tracks if the item has been spawned after collection
    private GameObject collectedItem; // The reference to the collected item prefab

    // Detect collisions with the item
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player touches an item and it's tagged as "Collectible"
        if (other.gameObject.CompareTag("Collectible") && !hasItem)
        {
            // Store the collected item prefab (ensure the item prefab is assigned)
            collectedItem = other.gameObject;
            // Destroy the item from the scene after collecting it
            Destroy(other.gameObject);
            // Mark that the player has collected the item
            hasItem = true;
            itemSpawned = false; // Reset the spawned status
            
        }
    }

    void Update()
    {
        // Check if the player presses "E", has collected an item, and hasn't spawned it yet
        if (hasItem && !itemSpawned && Input.GetButtonDown("P1Interact"))
        {
            // Spawn the collected item at the player's position
            Instantiate(itemPrefab, transform.position + transform.forward, Quaternion.identity);

            // Mark the item as spawned so it can't be spawned again
            itemSpawned = true;

            // The player no longer has the item, it has been used/spawned
            hasItem = false;
        }
    }
}
