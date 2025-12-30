using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FinishTrigger : MonoBehaviourPun
{
    [Header("Door Settings")]
    public Transform door;
    public float doorCloseAngle = 0f;   // Final rotation when closed
    public float doorOpenAngle = 108f;  // Initial rotation
    public float doorCloseSpeed = 2f;   // Smooth rotation speed

    private bool treasureDelivered = false;

    private SPInteractionUI interactionUI;

    void Start()
    {
        interactionUI = FindObjectOfType<SPInteractionUI>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        var spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

        // Show Return-text only if holding treasure and not delivered
        if (!treasureDelivered && spCollector.GetCurrentItem() == "Treasure")
        {
            spCollector.GetInteractionUI()?.ShowReturn();

            // Deliver treasure
            if (Input.GetButtonDown("Fire1"))
            {
                DeliverTreasure(spCollector);
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

        var spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

        // Always hide UI when leaving
        spCollector.GetInteractionUI()?.HideAll();
    }

    private void DeliverTreasure(SmallPlayerItemCollector spCollector)
    {
        treasureDelivered = true;

        // Consume treasure and hide its UI
        spCollector.ConsumeItem("Treasure");
        spCollector.HideTreasureUI();

        // Hide the Return-text immediately after delivery
        spCollector.GetInteractionUI()?.HideAll();

        // Close door smoothly
        if (door != null)
            StartCoroutine(CloseDoor());

        Debug.Log("Treasure delivered!");

        // Spawn new treasure on MasterClient
        if (!PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RPC_RequestNewTreasure), RpcTarget.MasterClient);
        else
            SpawnNewTreasure();
    }

    [PunRPC]
    private void RPC_RequestNewTreasure()
    {
        SpawnNewTreasure();
    }

    private void SpawnNewTreasure()
    {
        TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
        if (spawner != null)
        {
            // Reactivate the Treasure prefab instead of instantiating
            if (spawner.treasure != null)
                spawner.treasure.SetActive(true);

            // Teleport it to a random spawn point
            spawner.TeleportTreasure();

            Debug.Log("Treasure reactivated and teleported by MasterClient.");
        }
    }


    private IEnumerator CloseDoor()
    {
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

