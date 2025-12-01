using UnityEngine;
using Photon.Pun;

public class SmallPlayerItemCollector : MonoBehaviourPun
{
    [Header("Input Settings")]
    public string interactButton = "P1Interact";
    public string useButton = "Fire1"; // Default Unity Mouse1 mapping

    [Header("State")]
    private string currentItem = null;       // The name/type of collected item
    private GameObject itemInTrigger = null; // The item currently in trigger

    private SPBoppyPin boppyPinScript;
    private SPFlashbang flashBangScript;
    private SPSpring springScript;

    private void Awake()
    {
        // Cache the BoppyPin script if it's on this player
        boppyPinScript = GetComponent<SPBoppyPin>();
        flashBangScript = GetComponent<SPFlashbang>();
        springScript = GetComponent<SPSpring>();
    }

    private void Update()
    {
        // Only allow local player to handle input
        if (!photonView.IsMine) return;

        // Try picking up item
        if (itemInTrigger != null && Input.GetButtonDown(interactButton))
        {
            TryPickupItem();
        }

        // Try using item
        if (currentItem != null && Input.GetButtonDown(useButton))
        {
            UseCurrentItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // Detect collectibles by tag
        if (other.CompareTag("Collectible"))
        {
            itemInTrigger = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (itemInTrigger == other.gameObject)
        {
            itemInTrigger = null;
        }
    }

    private void TryPickupItem()
    {
        if (currentItem != null || itemInTrigger == null)
            return; // Already holding something or nothing to pick up

        currentItem = itemInTrigger.name.Replace("(Clone)", "").Trim();

        // Destroy item network-wide
        PhotonView itemPhotonView = itemInTrigger.GetComponent<PhotonView>();
        if (itemPhotonView != null && itemPhotonView.IsMine)
        {
            PhotonNetwork.Destroy(itemInTrigger);
        }
        else if (itemPhotonView != null)
        {
            photonView.RPC(nameof(RequestItemDestroyRPC), itemPhotonView.Owner, itemPhotonView.ViewID);
        }

        itemInTrigger = null;
        Debug.Log($"Picked up {currentItem}");
    }

    private void UseCurrentItem()
    {
        Debug.Log($"Used {currentItem}");

        switch (currentItem)
        {
            case "BoppyPinCollectible":
            case "BoppyPin":
                if (boppyPinScript != null)
                {
                    boppyPinScript.SpawnBoppyPin();
                }
                break;

            case "FlashBangCollectible":
                if (flashBangScript != null)
                {
                    flashBangScript.SpawnFlashbang();
                }
                break;

            case "SpringCollectible":
                if (springScript != null)
                {
                    springScript.UseSpring();
                }
                break;
        }

        // Clear after use
        currentItem = null;
    }

    [PunRPC]
    private void RequestItemDestroyRPC(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (itemView != null && itemView.IsMine)
        {
            PhotonNetwork.Destroy(itemView.gameObject);
        }
    }
}

