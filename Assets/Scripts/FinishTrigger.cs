using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class FinishTrigger : MonoBehaviourPun, IPunObservable
{
    [Header("Door Settings")]
    public Transform door;
    public float doorCloseAngle = 0f;   // Final rotation when closed
    public float doorOpenAngle = 108f;  // Initial rotation
    public float doorCloseSpeed = 2f;   // Smooth rotation speed

    private SPInteractionUI interactionUI;

    [SerializeField] private bool doorClosed;
    [SerializeField] private bool treasureDeliveredHere;
    [SerializeField] private bool doorUsed;

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
        if (spCollector.GetCurrentItem() == "Treasure" && !doorUsed)
        {
            spCollector.GetInteractionUI()?.ShowReturn();

            if (Input.GetButtonDown("Fire1"))
            {
                DeliverTreasure(spCollector);
            }
        }
        else if (!doorClosed &&
         TreasureScoreManager.Instance.treasuresDelivered >=
         TreasureScoreManager.Instance.totalTreasures &&
         !doorUsed)

        {
            // Show Enter for everyone
            spCollector.GetInteractionUI()?.ShowEnter();

            // Let any SP client press E to escape
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Call RPC_Escape on this player for all clients
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
        if (!spCollector.photonView.IsMine) return;

        photonView.RPC(nameof(RPC_RequestTreasureDelivery), RpcTarget.MasterClient, spCollector.photonView.ViewID);
    }

    private void RegisterTreasureDelivery()
    {
        // Tell MasterClient to add a treasure
        TreasureScoreManager.Instance.photonView
            .RPC(nameof(TreasureScoreManager.RPC_AddTreasure), RpcTarget.MasterClient);

        photonView.RPC(nameof(RPC_SpawnNewTreasure), RpcTarget.All);

        if (door != null && !doorClosed)
            photonView.RPC(nameof(RPC_CloseDoor), RpcTarget.All);

    }

    [PunRPC]
    private void RPC_RequestTreasureDelivery(int playerViewID)
    {
        if (doorUsed) return;   // Prevent double delivery

        doorUsed = true;
        treasureDeliveredHere = true;

        // Remove treasure from player hand
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            playerPV.RPC(nameof(SmallPlayerItemCollector.RPC_OnTreasureDelivered), RpcTarget.All);
        }

        // Update treasure count
        TreasureScoreManager.Instance.photonView
            .RPC(nameof(TreasureScoreManager.RPC_AddTreasure), RpcTarget.MasterClient);

        // Close the SPFinish door for all clients
        if (door != null)
            photonView.RPC(nameof(RPC_CloseDoor), RpcTarget.All);

        // Only MasterClient handles treasure respawn via TreasureSpawner
        if (PhotonNetwork.IsMasterClient)
        {
            TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
            if (spawner != null)
            {
                spawner.TeleportTreasure();   // Reactivate and move treasure
            }
        }
    }

    [PunRPC]
    private void RPC_SpawnNewTreasure()
    {
        if (!PhotonNetwork.IsMasterClient) return;  // Only MasterClient controls the treasure

        TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
        if (spawner != null && spawner.treasure != null)
        {
            spawner.treasure.SetActive(true);    // Reactivate
            spawner.TeleportTreasure();           // Move to new location

            // Optional: sync treasure object across clients
            PhotonView pv = spawner.treasure.GetComponent<PhotonView>();
            if (pv != null)
                pv.RPC("RPC_DeactivateTreasure", RpcTarget.AllBuffered);  // Or create a proper RPC to activate
        }
    }

    private IEnumerator CloseDoor()
    {
        if (door == null || doorClosed) yield break;

        doorClosed = true;

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(doorClosed);
            stream.SendNext(treasureDeliveredHere);
            stream.SendNext(doorUsed);
        }
        else
        {
            doorClosed = (bool)stream.ReceiveNext();
            treasureDeliveredHere = (bool)stream.ReceiveNext();
            doorUsed = (bool)stream.ReceiveNext();
        }
    }
    [PunRPC]
    private void RPC_CloseDoor()
    {
        StartCoroutine(CloseDoor());
    }


}

