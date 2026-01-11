using UnityEngine;
using Photon.Pun;

public class SmallPlayerItemCollector : MonoBehaviourPun
{
    [Header("Input Settings")]
    public string interactButton = "P1Interact";
    public string useButton = "Fire1"; // Default Unity Mouse1 mapping

    [Header("State")]
    private string currentItem = null;       // Name/type of collected item
    private GameObject itemInTrigger = null; // Item currently in trigger

    private SPBoppyPin boppyPinScript;
    private SPFlashbang flashBangScript;
    private SPSpring springScript;

    private GameObject boppyPinUIImage;
    private GameObject flashbangUIImage;
    private GameObject springUIImage;
    private GameObject treasureUIImage;

    private SPInteractionUI interactionUI;

    private void Awake()
    {
        // Cache item scripts if attached
        boppyPinScript = GetComponent<SPBoppyPin>();
        flashBangScript = GetComponent<SPFlashbang>();
        springScript = GetComponent<SPSpring>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        // Cache UI
        Canvas canvas = FindObjectOfType<Canvas>();
        boppyPinUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/BoppyPin")?.gameObject;
        flashbangUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Flashbang")?.gameObject;
        springUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Spring")?.gameObject;
        treasureUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Treasure")?.gameObject;

        interactionUI = FindObjectOfType<SPInteractionUI>();

        boppyPinUIImage.SetActive(false);
        flashbangUIImage.SetActive(false);
        springUIImage.SetActive(false);
        treasureUIImage.SetActive(false);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // Pick up item
        if (itemInTrigger != null && Input.GetButtonDown(interactButton))
        {
            TryPickupItem();
        }

        // Use item
        if (currentItem != null && Input.GetButtonDown(useButton))
        {
            UseCurrentItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.CompareTag("Collectible") && currentItem == null)
        {
            itemInTrigger = other.gameObject;
            interactionUI?.ShowTake();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (itemInTrigger == other.gameObject)
        {
            itemInTrigger = null;
            interactionUI?.HideAll();
        }
    }

    private void TryPickupItem()
    {
        if (currentItem != null || itemInTrigger == null) return;

        string itemName = itemInTrigger.name.Replace("(Clone)", "").Trim();

        if (itemInTrigger.CompareTag("Collectible") && itemInTrigger.name.Contains("Treasure"))
        {
            currentItem = "Treasure";
            treasureUIImage.SetActive(true);
            Debug.Log("Picked up the Treasure!");

            // Deactivate treasure across all clients
            PhotonView pv = itemInTrigger.GetComponent<PhotonView>();
            if (pv != null)
                pv.RPC("RPC_DeactivateTreasure", RpcTarget.AllBuffered);
            else
                itemInTrigger.SetActive(false);
        }
        else
        {
            // Other collectibles
            currentItem = itemName;
            Debug.Log($"Picked up {currentItem}");

            if (itemName.Contains("BoppyPinCollectible"))
                boppyPinUIImage.SetActive(true);
            if (itemName.Contains("FlashBangCollectible"))
                flashbangUIImage.SetActive(true);
            if (itemName.Contains("SpringCollectible"))
                springUIImage.SetActive(true);

            PhotonView itemPhotonView = itemInTrigger.GetComponent<PhotonView>();
            if (itemPhotonView != null && itemPhotonView.IsMine)
                PhotonNetwork.Destroy(itemInTrigger);
            else if (itemPhotonView != null)
                photonView.RPC(nameof(RequestItemDestroyRPC), itemPhotonView.Owner, itemPhotonView.ViewID);
        }

        itemInTrigger = null;
        interactionUI?.HideAll();
    }

    private void UseCurrentItem()
    {
        if (currentItem == null) return;

        switch (currentItem)
        {
            case "BoppyPinCollectible":
            case "BoppyPin":
                boppyPinScript?.SpawnBoppyPin();
                boppyPinUIImage.SetActive(false);
                currentItem = null;
                break;

            case "FlashBangCollectible":
                flashBangScript?.SpawnFlashbang();
                flashbangUIImage.SetActive(false);
                currentItem = null;
                break;

            case "SpringCollectible":
                springScript?.UseSpring();
                springUIImage.SetActive(false);
                currentItem = null;
                break;

            case "Treasure":
                Debug.Log("Holding Treasure. Go to SPFinish to deliver.");
                break;
        }
    }

    [PunRPC]
    private void RequestItemDestroyRPC(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (itemView != null && itemView.IsMine)
            PhotonNetwork.Destroy(itemView.gameObject);
    }

    [PunRPC]
    public void RPC_DeactivateTreasure()
    {
        gameObject.SetActive(false);
    }

    // Expose current item to other scripts
    public string GetCurrentItem() => currentItem;

    // Consume current item if it matches a specific type
    public void ConsumeItem(string itemName)
    {
        if (currentItem == itemName)
            currentItem = null;
    }

    public void HideTreasureUI()
    {
        if (treasureUIImage != null)
            treasureUIImage.SetActive(false);
    }

    public SPInteractionUI GetInteractionUI()
    {
        return interactionUI;
    }
}

