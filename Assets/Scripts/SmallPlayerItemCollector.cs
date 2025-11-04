using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;  

public class SmallPlayerItemCollector : MonoBehaviourPun
{
    public enum ItemType
    {
        None,
        BoppyPin,
        Flashbang,
        Spring
    }

    private ItemType currentItem = ItemType.None;
    private bool itemUsed = false;
    private GameObject collectedItem;
    private bool canPickUp = false;

    [Header("UI References")]
    public TextMeshProUGUI pickUpText;
    public Image boppyPinItemImage;
    public Image flashbangItemImage;
    public Image springItemImage;

    [Header("Item Scripts")]
    public SPBoppyPin spBoppyPin;
    public SPFlashbang spFlashBang;
    public SPSpring spSpring;

    void Start()
    {
        // Disable UI at start
        pickUpText.enabled = false;
        boppyPinItemImage.enabled = false;
        flashbangItemImage.enabled = false;
        springItemImage.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the local player should run pickup logic
        if (!photonView.IsMine) return;
        Debug.Log("Entered trigger with: " + other.name);
        if (other.gameObject.CompareTag("Collectible") && currentItem == ItemType.None)
        {
            collectedItem = other.gameObject;
            canPickUp = true;
            pickUpText.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.CompareTag("Collectible") && currentItem == ItemType.None)
        {
            canPickUp = false;
            collectedItem = null;
            pickUpText.enabled = false;
        }
    }

    void Update()
    {
        // 🔒 Ensure only local player handles input
        if (!photonView.IsMine) return;

        // ----------- ITEM PICKUP -----------
        if (canPickUp && currentItem == ItemType.None && Input.GetButtonDown("P1Interact"))
        {
            if (collectedItem != null)
            {
                // Check what kind of item this is by finding its child tag
                Transform boppyPinChild = collectedItem.transform.Find("BoppyPin");
                if (boppyPinChild != null && boppyPinChild.CompareTag("BoppyPin"))
                {
                    currentItem = ItemType.BoppyPin;
                    boppyPinItemImage.enabled = true;
                    Debug.Log("Player picked up a Boppy Pin.");
                }

                Transform flashbangChild = collectedItem.transform.Find("Flashbang");
                if (flashbangChild != null && flashbangChild.CompareTag("Flashbang"))
                {
                    currentItem = ItemType.Flashbang;
                    flashbangItemImage.enabled = true;
                    Debug.Log("Player picked up a Flashbang.");
                }

                Transform springChild = collectedItem.transform.Find("SPSpringCollectible");
                if (springChild != null && springChild.CompareTag("SPSpringCollectible"))
                {
                    currentItem = ItemType.Spring;
                    springItemImage.enabled = true;
                    Debug.Log("Player picked up a Spring.");
                }

                // ❗ Destroy for everyone on the network
                PhotonNetwork.Destroy(collectedItem);
            }

            itemUsed = false;
            collectedItem = null;
            canPickUp = false;
            pickUpText.enabled = false;
        }

        // ----------- ITEM USE -----------
        if (currentItem != ItemType.None && !itemUsed && Input.GetButtonDown("P1UseItem"))
        {
            if (currentItem == ItemType.BoppyPin)
            {
                spBoppyPin.SpawnBoppyPin();   // should use PhotonNetwork.Instantiate inside
                Debug.Log("SpawnBoppyPin() called.");
            }
            else if (currentItem == ItemType.Flashbang)
            {
                spFlashBang.SpawnFlashbang();  // should use PhotonNetwork.Instantiate inside
                Debug.Log("SpawnFlashbang() called.");
            }
            else if (currentItem == ItemType.Spring)
            {
                spSpring.UseSpring();          // should use PhotonNetwork.Instantiate or RPC
                Debug.Log("UseSpring() called.");
            }

            itemUsed = true;
            currentItem = ItemType.None;
            boppyPinItemImage.enabled = false;
            flashbangItemImage.enabled = false;
            springItemImage.enabled = false;
        }
    }
}

