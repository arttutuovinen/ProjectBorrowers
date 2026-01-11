using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class FinishTrigger : MonoBehaviourPun
{
    [Header("Door Settings")]
    public Transform door;
    public float doorCloseAngle = 0f;   // Final rotation when closed
    public float doorOpenAngle = 108f;  // Initial rotation
    public float doorCloseSpeed = 2f;   // Smooth rotation speed

    [Header("Treasure State")]
    public int treasuresDelivered = 0;
    public int totalTreasures = 3;

    private bool doorClosed = false;

    private SPInteractionUI interactionUI;

    // Keep track of which players have already delivered
    private readonly System.Collections.Generic.HashSet<int> playersWhoDelivered = new();

    void Start()
    {
        interactionUI = FindObjectOfType<SPInteractionUI>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SmallPlayerItemCollector spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

        // If player already delivered treasure, ignore
        if (playersWhoDelivered.Contains(spCollector.photonView.ViewID)) return;

        // 1️⃣ If player is holding treasure → show Return
        if (spCollector.GetCurrentItem() == "Treasure")
        {
            spCollector.GetInteractionUI()?.ShowReturn();

            if (Input.GetButtonDown("Fire1"))
            {
                DeliverTreasure(spCollector);
            }
        }
        // 2️⃣ Show Enter if all treasures collected and door not closed
        else if (!doorClosed && treasuresDelivered >= totalTreasures)
        {
            spCollector.GetInteractionUI()?.ShowEnter();

            if (Input.GetKeyDown(KeyCode.E))
            {
                spCollector.gameObject.SetActive(false);
            }
        }
        else
        {
            spCollector.GetInteractionUI()?.HideAll();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SmallPlayerItemCollector spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

        spCollector.GetInteractionUI()?.HideAll();
    }

    private void DeliverTreasure(SmallPlayerItemCollector spCollector)
    {
        // Prevent multiple deliveries per player
        if (playersWhoDelivered.Contains(spCollector.photonView.ViewID)) return;

        // Mark player as delivered
        playersWhoDelivered.Add(spCollector.photonView.ViewID);

        // Local visuals
        spCollector.ConsumeItem("Treasure");
        spCollector.HideTreasureUI();
        spCollector.GetInteractionUI()?.HideAll();

        // MasterClient handles authoritative treasure delivery
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_RequestTreasureDelivery), RpcTarget.MasterClient, spCollector.photonView.ViewID);
        }
        else
        {
            RegisterTreasureDelivery();
        }

        // Close door smoothly
        if (door != null && !doorClosed)
            StartCoroutine(CloseDoor());
    }

    [PunRPC]
    private void RPC_RequestTreasureDelivery(int playerViewID)
    {
        // Mark player as delivered on MasterClient
        playersWhoDelivered.Add(playerViewID);
        RegisterTreasureDelivery();
    }

    private void RegisterTreasureDelivery()
    {
        treasuresDelivered++;

        // Update UI for all clients
        photonView.RPC(nameof(RPC_UpdateTreasureUI), RpcTarget.All, treasuresDelivered);

        // Spawn a new treasure if needed (MasterClient only)
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnNewTreasure();
        }

        // If all treasures delivered, mark door closed
        if (treasuresDelivered >= totalTreasures)
        {
            doorClosed = true;
        }

        Debug.Log($"Treasures Delivered: {treasuresDelivered}/{totalTreasures}");
    }

    [PunRPC]
    private void RPC_UpdateTreasureUI(int value)
    {
        // Update UI for all SmallPlayer clients
        SmallPlayerItemCollector[] allPlayers = FindObjectsOfType<SmallPlayerItemCollector>();
        foreach (var sp in allPlayers)
        {
            var uiText = sp.GetComponentInChildren<TextMeshProUGUI>();
            if (uiText != null)
                uiText.text = $"Treasures: {value}/{totalTreasures}";
        }
    }

    private void SpawnNewTreasure()
    {
        TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
        if (spawner != null && spawner.treasure != null)
        {
            spawner.treasure.SetActive(true);
            spawner.TeleportTreasure();
            Debug.Log("Treasure reactivated and teleported by MasterClient.");
        }
    }

    private IEnumerator CloseDoor()
    {
        if (door == null) yield break;

        float t = 0f;
        Quaternion startRot = door.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, doorCloseAngle, 0f);

        while (t < 1f)
        {
            t += Time.deltaTime * doorCloseSpeed;
            door.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        door.localRotation = targetRot;
    }
}

