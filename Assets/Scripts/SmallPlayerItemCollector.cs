using UnityEngine;
using Photon.Pun;
using TMPro;

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
    private SPScissors scissorsScript;

    private GameObject boppyPinUIImage;
    private GameObject flashbangUIImage;
    private GameObject springUIImage;
    private GameObject treasureUIImage;
    private GameObject scissorsUIImage;
    private TextMeshProUGUI treasureText;

    private SPInteractionUI interactionUI;
    private GameObject escapeText;

    //Audio
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ítemSound;
    [SerializeField] private AudioClip flashbangSound;
    [SerializeField] private AudioClip springSound;
    [SerializeField] private AudioClip boppyPinSound;
    [SerializeField] private AudioClip treasureSound;

    private void Awake()
    {
        // Cache item scripts if attached
        boppyPinScript = GetComponent<SPBoppyPin>();
        flashBangScript = GetComponent<SPFlashbang>();
        springScript = GetComponent<SPSpring>();
        scissorsScript = GetComponent<SPScissors>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        // Cache UI
        Canvas canvas = FindObjectOfType<Canvas>();
        boppyPinUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/BoppyPin")?.gameObject;
        flashbangUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Flashbang")?.gameObject;
        springUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Spring")?.gameObject;
        scissorsUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Scissors")?.gameObject;
        treasureUIImage = canvas.transform.Find("SmallPlayerUI/ItemHolder/Treasure")?.gameObject;
        treasureText = canvas.transform.Find("SmallPlayerUI/Treasures").GetComponent<TextMeshProUGUI>();
        escapeText = canvas.transform.Find("SmallPlayerUI/Escape")?.gameObject;
        if (escapeText != null) escapeText.SetActive(false);

        interactionUI = FindObjectOfType<SPInteractionUI>();

        boppyPinUIImage.SetActive(false);
        flashbangUIImage.SetActive(false);
        springUIImage.SetActive(false);
        treasureUIImage.SetActive(false);
        scissorsUIImage.SetActive(false);
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
            audioSource.PlayOneShot(treasureSound);
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
            audioSource.PlayOneShot(ítemSound);
            if (itemName.Contains("BoppyPinCollectible"))
                boppyPinUIImage.SetActive(true);
            if (itemName.Contains("FlashBangCollectible"))
                flashbangUIImage.SetActive(true);
            if (itemName.Contains("SpringCollectible"))
                springUIImage.SetActive(true);
            if (itemName.Contains("ScissorsCollectible"))
            {
                scissorsUIImage.SetActive(true);
                scissorsScript?.GotScissors();
            }
                


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
                UseBoppyPin();
                break;

            case "FlashBangCollectible":
                UseFlashbang();
                break;

            case "SpringCollectible":
                UseSpring();
                break;

            case "ScissorsCollectible":
                UseScissors();
                break;

            case "Treasure":
                Debug.Log("Holding Treasure. Go to SPFinish to deliver.");
                break;
        }
    }

    private void UseBoppyPin()
    {
        boppyPinScript?.SpawnBoppyPin();
        boppyPinUIImage.SetActive(false);

        PlayBoppyPinSound();

        currentItem = null;
    }

    private void PlayBoppyPinSound()
    {
        audioSource.PlayOneShot(boppyPinSound);
        photonView.RPC(nameof(RPC_PlayBoppyPinSound), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PlayBoppyPinSound()
    {
        audioSource.PlayOneShot(boppyPinSound);
    }

    private void UseFlashbang()
    {
        flashBangScript?.SpawnFlashbang();
        flashbangUIImage.SetActive(false);

        PlayFlashbangSound();

        currentItem = null;
    }

    private void PlayFlashbangSound()
    {
        audioSource.PlayOneShot(flashbangSound);
        photonView.RPC(nameof(RPC_PlayFlashbangSound), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PlayFlashbangSound()
    {
        audioSource.PlayOneShot(flashbangSound);
    }

    private void UseSpring()
    {
        springScript?.UseSpring();
        springUIImage.SetActive(false);

        PlaySpringSound();

        currentItem = null;
    }

    private void PlaySpringSound()
    {
        audioSource.PlayOneShot(springSound);
        photonView.RPC(nameof(RPC_PlaySpringSound), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PlaySpringSound()
    {
        audioSource.PlayOneShot(springSound);
    }

    private void UseScissors()
    {
        scissorsScript?.UseScissors();
        scissorsScript?.UsedScissors();
        scissorsUIImage.SetActive(false);

        PlayScissorsSound();

        currentItem = null;
    }

    private void PlayScissorsSound()
    {
        //audioSource.PlayOneShot(springSound);
        //photonView.RPC(nameof(RPC_PlayScissorsSound), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PlayScissorsSound()
    {
        //audioSource.PlayOneShot(springSound);
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

    public void SetTreasureCount(int value, int max)
    {
        if (treasureText != null)
            treasureText.text = $"Treasures: {value}/{max}";
    }

    // Add a method to update Escape visibility
    public void SetEscapeActive(bool active)
    {
        if (escapeText != null)
            escapeText.SetActive(active);
    }

    [PunRPC]
    public void RPC_Escape()
    {
        gameObject.SetActive(false);

        if (photonView.IsMine)
        {
            interactionUI?.HideAll();

            // REPORT ESCAPE
            SPWinManager.Instance.photonView.RPC(
                nameof(SPWinManager.RPC_ReportEscaped),
                RpcTarget.MasterClient
            );
        }
    }
    [PunRPC]
    public void RPC_OnTreasureDelivered()
    {
        currentItem = null;
        HideTreasureUI();
        interactionUI?.HideAll();
    }

    [PunRPC]
    public void RPC_DropTreasureOnCapture()
    {
        if (currentItem != "Treasure") return;

        currentItem = null;
        treasureUIImage.SetActive(false);

        // Ask MasterClient to respawn the treasure
        photonView.RPC("RPC_RequestTreasureRespawn", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_RequestTreasureRespawn()
    {
        TreasureSpawner spawner = FindFirstObjectByType<TreasureSpawner>();
        if (spawner != null)
        {
            spawner.TeleportTreasure();
        }
    }
}

