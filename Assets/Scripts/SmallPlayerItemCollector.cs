using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SmallPlayerItemCollector : MonoBehaviour
{
    public enum ItemType
    {
        None,
        BoppyPin,
        Flashbang
    }

    private ItemType currentItem = ItemType.None;  // Tracks the type of the currently held item
    private bool itemUsed = false; // Tracks if the item has been used after collection
    private GameObject collectedItem; // The reference to the collectible item in range
    private bool canPickUp = false;

    public TextMeshProUGUI pickUpText;  // Reference to the TextMeshPro UI element
    public Image boppyPinItemImage;  // Reference to the UI Image for Boppy Pin
    public Image flashbangItemImage;  // Reference to the UI Image for Flashbang

    public SPBoppyPin spBoppyPin; // Reference to another script that handles Boppy Pin behavior.
    public SPFlashbang spFlashBang; // Reference to another script that handles Flashbang behavior.

    void Start()
    {
        // Disable the pickup text and item images at the start of the game
        pickUpText.enabled = false;
        boppyPinItemImage.enabled = false;
        flashbangItemImage.enabled = false;
    }

    // Detect when the player enters the collider of a collectible item
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player is near a collectible item and not already holding an item
        if (other.gameObject.CompareTag("Collectible") && currentItem == ItemType.None)
        {
            // Store the reference to the collectible item the player can pick up
            collectedItem = other.gameObject;
            canPickUp = true;  // Player can now pick up the item
            pickUpText.enabled = true;  // Show the pickup text
        }
    }

    // Detect when the player leaves the collider of the collectible item
    private void OnTriggerExit(Collider other)
    {
        // If the player moves away from the collectible item, they can no longer pick it up
        if (other.gameObject.CompareTag("Collectible") && currentItem == ItemType.None)
        {
            canPickUp = false;
            collectedItem = null; // Reset the collectible reference
            pickUpText.enabled = false;
        }
    }

    void Update()
    {
        // Check if the player is in range to pick up the item and presses "P1Interact"
        if (canPickUp && currentItem == ItemType.None && Input.GetButtonDown("P1Interact"))
        {
            // Look for a child object with specific tags to determine the type of the item
            if (collectedItem != null)
            {
                // Try to find a child tagged as "BoppyPin"
                Transform boppyPinChild = collectedItem.transform.Find("BoppyPin");
                if (boppyPinChild != null && boppyPinChild.CompareTag("BoppyPin"))
                {
                    currentItem = ItemType.BoppyPin;
                    boppyPinItemImage.enabled = true;
                    Debug.Log("Player picked up a Boppy Pin.");
                }

                // Try to find a child tagged as "Flashbang"
                Transform flashbangChild = collectedItem.transform.Find("Flashbang");
                if (flashbangChild != null && flashbangChild.CompareTag("Flashbang"))
                {
                    currentItem = ItemType.Flashbang;
                    flashbangItemImage.enabled = true;
                    Debug.Log("Player picked up a Flashbang.");
                }
            }

            // Destroy the parent collectible item and reset pickup state
            Destroy(collectedItem);
            itemUsed = false;
            collectedItem = null;
            canPickUp = false;
            pickUpText.enabled = false;
        }

        // Check if the player presses "P1UseItem", has collected an item, and hasn't used it yet
        if (currentItem != ItemType.None && !itemUsed && Input.GetButtonDown("P1UseItem"))
        {
            // Call the appropriate method based on the currently held item type
            if (currentItem == ItemType.BoppyPin)
            {
                spBoppyPin.SpawnBoppyPin();
                Debug.Log("SpawnBoppyPin() method called.");
            }
            else if (currentItem == ItemType.Flashbang)
            {
                spFlashBang.SpawnFlashbang();
                Debug.Log("SpawnFlashbang() method called.");
            }

            // Mark the item as used and reset state
            itemUsed = true;
            currentItem = ItemType.None;
            boppyPinItemImage.enabled = false;
            flashbangItemImage.enabled = false;
        }
    }
}
