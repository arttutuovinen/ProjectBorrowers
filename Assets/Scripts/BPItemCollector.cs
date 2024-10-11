using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BPItemCollector : MonoBehaviour
{
    public enum ItemType
    {
        None,
        ThrowingItem,
        OtherItem
    }
    public float rayDistance = 10f; // The distance of the raycast
    public LayerMask collectibleLayer; // Layer mask to specify which layers should be considered for the raycast
    public Camera playerCamera;  // Reference to the Big Player's camera (assign via Inspector)
    private ItemType currentItem = ItemType.None;  // Tracks the type of the currently held item
    private bool itemUsed = false; // Tracks if the item has been used after collection
    private GameObject collectedItem; // The reference to the collectible item in range
    private bool canPickUp = false;

    public TextMeshProUGUI bpPickUpText;  // Reference to the TextMeshPro UI element
    public Image throwingItem;  // Reference to the UI Image for Boppy Pin
    //public Image otherItemImage;  // Reference to the UI Image for Other Item

    public BPThrowItem bpThrowItem; // Reference to another script that handles Boppy Pin behavior.
    public SPBoppyPin bpOtherItem; // Reference to another script that handles Other Item behavior.

    void Start()
    {
        // Disable the pickup text and item images at the start of the game
        bpPickUpText.enabled = false;
        throwingItem.enabled = false;
        //otherItemImage.enabled = false;
    }

    void Update()
    {
        // Check if the player is aiming at a collectible item using a raycast
        CheckForCollectible();

        // Check if the player presses the interact button when in range of a collectible item
        if (canPickUp && currentItem == ItemType.None && Input.GetButtonDown("P2Interact"))
        {
            PickUpItem();
        }

        // Check if the player presses "P2UseItem", has collected an item, and hasn't used it yet
        if (currentItem != ItemType.None && !itemUsed && Input.GetButtonDown("P2UseItem"))
        {
            UseItem();
        }
    }

    void CheckForCollectible()
    {
        // Cast a ray from the center of the player's camera
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Perform the raycast and check if it hits a collider on the collectible layer
        if (Physics.Raycast(ray, out hit, rayDistance, collectibleLayer))
        {
            // Check if the hit object has the correct tag and the player doesn't already have an item
            if (hit.collider.CompareTag("BPCollectible") && currentItem == ItemType.None)
            {
                // Store the reference to the collectible item
                collectedItem = hit.collider.gameObject;
                canPickUp = true; // Allow the player to pick up the item
                bpPickUpText.enabled = true; // Show the pickup text
                return;
            }
        }

        // If the raycast does not hit any collectible, disable the pickup option
        canPickUp = false;
        collectedItem = null; // Reset the reference to the collectible item
        bpPickUpText.enabled = false; // Hide the pickup text
    }

    void PickUpItem()
    {
        // Look for a child object with specific tags to determine the type of the item
        if (collectedItem != null)
        {
            // Try to find a child tagged as "BPThrowItem"
            Transform throwItemChild = collectedItem.transform.Find("BPThrowItem");
            if (throwItemChild != null && throwItemChild.CompareTag("BPThrowItem"))
            {
                currentItem = ItemType.ThrowingItem;
                throwingItem.enabled = true;
                Debug.Log("Player picked up a Boppy Pin.");
            }

            // Try to find a child tagged as "BPOtherItem"
            Transform otherItemChild = collectedItem.transform.Find("BPOtherItem");
            if (otherItemChild != null && otherItemChild.CompareTag("BPOtherItem"))
            {
                currentItem = ItemType.OtherItem;
                //otherItemImage.enabled = true;
                Debug.Log("Player picked up an Other Item.");
            }

            // Destroy the parent collectible item and reset pickup state
            Destroy(collectedItem);
            itemUsed = false;
            collectedItem = null;
            canPickUp = false;
            bpPickUpText.enabled = false;
        }
    }

    void UseItem()
    {
        // Call the appropriate method based on the currently held item type
        if (currentItem == ItemType.ThrowingItem)
        {
            bpThrowItem.SpawnThrowItem();
            Debug.Log("SpawnBoppyPin() method called.");
        }
        else if (currentItem == ItemType.OtherItem)
        {
            //bpOtherItem.SpawnItem();
            Debug.Log("SpawnOtherItem() method called.");
        }

        // Mark the item as used and reset state
        itemUsed = true;
        currentItem = ItemType.None;
        throwingItem.enabled = false;
        //otherItemImage.enabled = false;
    }
}
