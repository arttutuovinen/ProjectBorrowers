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

    private bool doorClosed = false;

    private SPInteractionUI interactionUI;
    private bool doorUsed = false;
    private bool treasureDeliveredHere = false;


    void Start()
    {
        interactionUI = FindObjectOfType<SPInteractionUI>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SmallPlayerItemCollector spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null) return;

        // 1️⃣ If player is holding treasure → show Return
        if (spCollector.GetCurrentItem() == "Treasure")
        {
            spCollector.GetInteractionUI()?.ShowReturn();

            if (Input.GetButtonDown("Fire1"))
            {
                DeliverTreasure(spCollector);
            }
        }
        // 2️⃣ If all treasures delivered AND this SPFinish hasn’t received a treasure → show Enter
        else if (!doorClosed &&
                 TreasureScoreManager.Instance.treasuresDelivered >=
                 TreasureScoreManager.Instance.totalTreasures &&
                 !treasureDeliveredHere)  // Only unused SPFinish
        {
            spCollector.GetInteractionUI()?.ShowEnter();
            if (Input.GetKeyDown(KeyCode.E) && spCollector.photonView.IsMine)
            {
                spCollector.photonView.RPC(nameof(SmallPlayerItemCollector.RPC_Escape), RpcTarget.All);
            }
        }
        // 3️⃣ Otherwise → hide interaction UI
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
        if (doorUsed) return;   // door can only accept one treasure
        doorUsed = true;

        // Mark this SPFinish as used
        treasureDeliveredHere = true;

        // Local visuals
        spCollector.ConsumeItem("Treasure");
        spCollector.HideTreasureUI();
        spCollector.GetInteractionUI()?.HideAll();

        // MasterClient handles authoritative treasure delivery
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_RequestTreasureDelivery), RpcTarget.MasterClient);
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
    private void RPC_RequestTreasureDelivery()
    {
        if (doorUsed) return;
        doorUsed = true;
        RegisterTreasureDelivery();
    }

    private void RegisterTreasureDelivery()
    {
        // Update the score
        TreasureScoreManager.Instance.photonView
            .RPC(nameof(TreasureScoreManager.RPC_AddTreasure), RpcTarget.MasterClient);

        // Reactivate and teleport treasure (MasterClient only)
        if (PhotonNetwork.IsMasterClient)
            SpawnNewTreasure();

        // Close door
        if (door != null && !doorClosed)
            StartCoroutine(CloseDoor());
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

