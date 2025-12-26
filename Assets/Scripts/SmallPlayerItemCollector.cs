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

        string itemName = itemInTrigger.name.Replace("(Clone)", "").Trim();

        // Treat Treasure separately
        if (itemInTrigger.CompareTag("Treasure"))
        {
            currentItem = "Treasure"; // You can use a specific name for treasure
            Debug.Log("Picked up the Treasure!");
        }
        else
        {
            currentItem = itemName;
            Debug.Log($"Picked up {currentItem}");
        }

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
    }

    private void UseCurrentItem()
    {
        if (currentItem == null) return;

        Debug.Log($"Used {currentItem}");

        switch (currentItem)
        {
            case "BoppyPinCollectible":
            case "BoppyPin":
                boppyPinScript?.SpawnBoppyPin();
                currentItem = null;
                break;

            case "FlashBangCollectible":
                flashBangScript?.SpawnFlashbang();
                currentItem = null;
                break;

            case "SpringCollectible":
                springScript?.UseSpring();
                currentItem = null;
                break;

            case "Treasure":
                // Do nothing here! Let FinishTrigger handle delivery
                Debug.Log("Holding Treasure. Go to SPFinish to deliver.");
                break;
        }
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

    // Expose current item to other scripts
    public string GetCurrentItem()
    {
        return currentItem;
    }

    // Consume the current item if it matches a specific type
    public void ConsumeItem(string itemName)
    {
        if (currentItem == itemName)
            currentItem = null;
    }
}

