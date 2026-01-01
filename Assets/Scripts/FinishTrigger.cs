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
    private SPScoreManager scoreManager;

    void Start()
    {
        interactionUI = FindObjectOfType<SPInteractionUI>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        var spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

        var scoreManager = spCollector.GetComponent<SPScoreManager>();
        var interactionUI = spCollector.GetInteractionUI();

        // 1️⃣ If player is holding treasure and hasn't delivered yet → show Return
        if (!treasureDelivered && spCollector.GetCurrentItem() == "Treasure")
        {
            interactionUI?.ShowReturn();

            if (Input.GetButtonDown("Fire1"))
            {
                DeliverTreasure(spCollector);
                scoreManager?.AddTreasure();
            }
        }
        // 2️⃣ Show Enter ONLY if all treasures collected AND this door hasn't been closed
        else if (!treasureDelivered && scoreManager != null && scoreManager.treasuresCollected >= scoreManager.totalTreasures)
        {
            interactionUI?.ShowEnter();

            if (Input.GetKeyDown(KeyCode.E))
            {
                spCollector.gameObject.SetActive(false);
            }
        }
        // 3️⃣ Otherwise hide UI
        else
        {
            interactionUI?.HideAll();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        var spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null || !spCollector.photonView.IsMine) return;

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

