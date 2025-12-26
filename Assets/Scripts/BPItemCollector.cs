using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        Medicine
    }

    public float rayDistance = 10f;
    public LayerMask collectibleLayer;
    public Camera playerCamera;

    private ItemType currentItem = ItemType.None;
    private bool itemUsed = false;
    private GameObject collectedItem;
    private bool canPickUp = false;

    // Components
    private BPThrowItem bpThrowItem;
    private BigPlayerAnimation bpAnimation;
    private BPMouseTrap bpMouseTrap;
    private BPCompass bpCompass;
    private BPVacuumCleaner bpVacuumCleaner;
    private BPTapeManager bpTapeManager;
    private BPMedicine bpMedicine;

    // ✔ All valid item tags
    private readonly HashSet<string> itemTags = new HashSet<string>
    {
        "BPThrowItem",
        "BPThrowItem2",
        "BPThrowItem3",
        "BPFlySwatter",
        "BPMouseTrap",
        "BPCompass",
        "BPVacuumCleaner",
        "BPTape",
        "BPMedicine"
    };

    void Start()
    {
        bpThrowItem = GetComponent<BPThrowItem>();
        bpAnimation = GetComponent<BigPlayerAnimation>();
        bpMouseTrap = GetComponent<BPMouseTrap>();
        bpCompass = GetComponent<BPCompass>();
        bpVacuumCleaner = GetComponent<BPVacuumCleaner>();
        bpTapeManager = GetComponent<BPTapeManager>();
        bpMedicine = GetComponent<BPMedicine>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        CheckForCollectible();

        if (canPickUp && currentItem == ItemType.None && Input.GetButtonDown("P1Interact"))
            PickUpItem();

        if (currentItem != ItemType.None && !itemUsed && Input.GetButtonDown("Fire1"))
            UseItem();
    }

    // -----------------------------------------------------------------------
    // ✔ Detect closest item using only specific item tags
    // -----------------------------------------------------------------------
    void CheckForCollectible()
    {
        collectedItem = null;
        canPickUp = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, collectibleLayer, QueryTriggerInteraction.Collide))
        {
            if (itemTags.Contains(hit.collider.tag))
            {
                collectedItem = hit.collider.gameObject;
                canPickUp = true;
            }
        }
    }

    // -----------------------------------------------------------------------
    // ✔ Determine item only by its tag
    // -----------------------------------------------------------------------
    void PickUpItem()
    {
        if (collectedItem == null) return;

        switch (collectedItem.tag)
        {
            case "BPThrowItem":
                currentItem = ItemType.ThrowingItem;
                break;

            case "BPThrowItem2":
                currentItem = ItemType.ThrowingItem_02;
                break;

            case "BPThrowItem3":
                currentItem = ItemType.ThrowingItem_03;
                break;

            case "BPFlySwatter":
                currentItem = ItemType.FlySwatter;
                break;

            case "BPMouseTrap":
                currentItem = ItemType.MouseTrap;
                break;

            case "BPCompass":
                currentItem = ItemType.Compass;
                break;

            case "BPVacuumCleaner":
                currentItem = ItemType.VacuumCleaner;
                break;

            case "BPTape":
                currentItem = ItemType.Tape;
                break;

            case "BPMedicine":
                currentItem = ItemType.Medicine;
                break;
        }

        PhotonView itemPV = collectedItem.GetComponentInParent<PhotonView>();
        if (itemPV != null)
        {
            itemPV.RPC("RPC_RequestDestroy", RpcTarget.MasterClient);
        }
        itemUsed = false;
        collectedItem = null;
        canPickUp = false;

        Debug.Log("Picked up: " + currentItem);
    }

    // -----------------------------------------------------------------------
    // ✔ Use item
    // -----------------------------------------------------------------------
    public void UseItem()
    {
        if (!photonView.IsMine) return; // Only the local player can use items

        switch (currentItem)
        {
            case ItemType.ThrowingItem:
                if (bpThrowItem != null) bpThrowItem.SpawnThrowItem();
                break;

            case ItemType.ThrowingItem_02:
                if (bpThrowItem != null) bpThrowItem.SpawnThrowItem2();
                break;

            case ItemType.ThrowingItem_03:
                if (bpThrowItem != null) bpThrowItem.SpawnThrowItem3();
                break;

            case ItemType.FlySwatter:
                if (bpAnimation != null) bpAnimation.FlySwatter();
                break;

            case ItemType.MouseTrap:
                if (bpMouseTrap != null) bpMouseTrap.SpawnMouseTrap();
                break;

            case ItemType.Compass:
                if (bpCompass != null) bpCompass.UseCompass();
                break;

            case ItemType.VacuumCleaner:
                if (bpVacuumCleaner != null) bpVacuumCleaner.TryPullTarget();
                break;

            case ItemType.Tape:
                if (bpTapeManager != null)
                {
                    if (!bpTapeManager.TryActivateTape())
                    {
                        // Don't consume item if activation failed
                        return;
                    }
                }
                break;

            case ItemType.Medicine:
                if (bpMedicine != null) bpMedicine.ActivateSpeedBoost();
                break;

            default:
                Debug.LogWarning("UseItem called but no valid currentItem selected.");
                return;
        }

        // Mark as used and reset
        itemUsed = true;
        currentItem = ItemType.None;
    }

}

