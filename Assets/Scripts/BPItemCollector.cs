using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class BPItemCollector : MonoBehaviourPun
{
    public enum ItemType
    {
        None,
        ThrowingItem,
        ThrowingItem_02,
        ThrowingItem_03,
        FlySwatter,
        MouseTrap,
        Compass,
        VacuumCleaner,
        Tape,
        Medicine,
    }
    public float rayDistance = 10f; // The distance of the raycast
    public LayerMask collectibleLayer; // Layer mask to specify which layers should be considered for the raycast
    public Camera playerCamera;  // Reference to the Big Player's camera (assign via Inspector)
    private ItemType currentItem = ItemType.None;  // Tracks the type of the currently held item
    private bool itemUsed = false; // Tracks if the item has been used after collection
    private GameObject collectedItem; // The reference to the collectible item in range
    private bool canPickUp = false;

    public TextMeshProUGUI bpPickUpText;
    public Image throwingItem;
    public Image flySwatterImage;
    public Image mouseTrapImage;
    public Image compassImage;
    public Image vacuumCleanerImage;
    public Image tapeImage;
    public Image medicineImage;

    public BPThrowItem bpThrowItem; 
    public BigPlayerAnimation bpAnimation;
    public BPMouseTrap bpMouseTrap;
    public BPCompass bpCompass;
    public BPVacuumCleaner bpVacuumCleaner;
    public BPTapeManager bpTapeManager;
    public BPMedicine bpMedicine;

    void Start()
    {
        bpPickUpText.enabled = false;
        throwingItem.enabled = false;
        flySwatterImage.enabled = false;
        mouseTrapImage.enabled = false;
        compassImage.enabled = false;
        vacuumCleanerImage.enabled = false;
        tapeImage.enabled = false;
        medicineImage.enabled = false;
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
        if (currentItem != ItemType.None && !itemUsed && Input.GetButtonDown("P2PickUp"))
        {
            UseItem();
        }
    }

    void CheckForCollectible()
    {
        // Cast a ray from the center of the player's camera
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, collectibleLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestCollectible = null;

        foreach (RaycastHit hit in hits)
        {
            // Check if the hit object has the correct tag and the player doesn't already have an item
            if (hit.collider.CompareTag("BPCollectible") && currentItem == ItemType.None)
            {
                // Update the closest collectible if it's nearer than the previous one
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestCollectible = hit.collider.gameObject;
                }
            }
        }

        // If a collectible was found, update the pickup state
        if (closestCollectible != null)
        {
            collectedItem = closestCollectible;
            canPickUp = true; // Allow the player to pick up the item
            bpPickUpText.enabled = true; // Show the pickup text
        }
        else
        {
            // If the raycast does not hit any collectible, disable the pickup option
            canPickUp = false;
            collectedItem = null; // Reset the reference to the collectible item
            bpPickUpText.enabled = false; // Hide the pickup text
        }
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
                Debug.Log("Player picked up a ThrowItem.");
            }
            
            Transform throwItem2Child = collectedItem.transform.Find("BPThrowItem2");
            if (throwItem2Child != null && throwItem2Child.CompareTag("BPThrowItem2"))
            {
                currentItem = ItemType.ThrowingItem_02;
                throwingItem.enabled = true;
                Debug.Log("Player picked up a ThrowItem_02.");
            }
           
            Transform throwItem3Child = collectedItem.transform.Find("BPThrowItem3");
            if (throwItem3Child != null && throwItem3Child.CompareTag("BPThrowItem3"))
            {
                currentItem = ItemType.ThrowingItem_03;
                throwingItem.enabled = true;
                Debug.Log("Player picked up a ThrowItem_03.");
            }
            
            Transform flyswatterChild = collectedItem.transform.Find("BPFlySwatter");
            if (flyswatterChild != null && flyswatterChild.CompareTag("BPFlySwatter"))
            {
                currentItem = ItemType.FlySwatter;
                flySwatterImage.enabled = true;
                Debug.Log("Player picked up an Other Item.");
            }
          
            Transform mouseTrapChild = collectedItem.transform.Find("BPMouseTrap");
            if (mouseTrapChild != null && mouseTrapChild.CompareTag("BPMouseTrap"))
            {
                currentItem = ItemType.MouseTrap;
                mouseTrapImage.enabled = true;
                Debug.Log("Player picked up an Other Item.");
            }
            Transform compassChild = collectedItem.transform.Find("BPCompass");
            if (compassChild != null && compassChild.CompareTag("BPCompass"))
            {
                currentItem = ItemType.Compass;
                compassImage.enabled = true;
                Debug.Log("Player picked up an Compass.");
            }
            Transform vacuumCleanerChild = collectedItem.transform.Find("BPVacuumCleaner");
            if (vacuumCleanerChild != null && vacuumCleanerChild.CompareTag("BPVacuumCleaner"))
            {
                currentItem = ItemType.VacuumCleaner;
                vacuumCleanerImage.enabled = true;
                Debug.Log("Player picked up a VacuumCleaner.");
            }
            Transform tapeChild = collectedItem.transform.Find("BPTape");
            if (tapeChild != null && tapeChild.CompareTag("BPTape"))
            {
                currentItem = ItemType.Tape;
                tapeImage.enabled = true;
                Debug.Log("Player picked up a Tape.");
            }
            Transform medicineChild = collectedItem.transform.Find("BPMedicine");
            if (medicineChild != null && medicineChild.CompareTag("BPMedicine"))
            {
                currentItem = ItemType.Medicine;
                medicineImage.enabled = true;
                Debug.Log("Player picked up a medicine.");
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
        }
        if (currentItem == ItemType.ThrowingItem_02)
        {
            bpThrowItem.SpawnThrowItem2();
        }
        if (currentItem == ItemType.ThrowingItem_03)
        {
            bpThrowItem.SpawnThrowItem3();
        }
        if (currentItem == ItemType.FlySwatter)
        {
            bpAnimation.FlySwatter();
        }
        if (currentItem == ItemType.MouseTrap)
        {
            bpMouseTrap.SpawnMouseTrap();
        }
        if (currentItem == ItemType.Compass)
        {
            bpCompass.UseCompass();
        }
        if (currentItem == ItemType.VacuumCleaner)
        {
            bpVacuumCleaner.TryPullTarget();
        }
        if (currentItem == ItemType.Tape)
        {
            bool tapePlaced = bpTapeManager.TryActivateTape();
            if (!tapePlaced)
            {
                // Do NOT consume the item if placement failed
                return;
            }
        }
        if (currentItem == ItemType.Medicine)
        {
            bpMedicine.ActivateSpeedBoost();
        }
        // Mark the item as used and reset state
        itemUsed = true;
        currentItem = ItemType.None;
        throwingItem.enabled = false;
        flySwatterImage.enabled = false;
        mouseTrapImage.enabled = false;
        compassImage.enabled = false;
        vacuumCleanerImage.enabled = false;
        tapeImage.enabled = false;
        medicineImage.enabled = false;
    }
}
