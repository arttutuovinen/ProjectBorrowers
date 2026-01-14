using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class FinishTrigger : MonoBehaviourPun, IPunObservable
{
    public Transform door;
    public float doorCloseAngle = 0f;
    public float doorCloseSpeed = 2f;

    private bool doorClosed;
    private bool doorUsed;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SmallPlayerItemCollector sp = other.GetComponent<SmallPlayerItemCollector>();
        if (sp == null || !sp.photonView.IsMine) return;

        // 1️⃣ Return treasure
        if (sp.GetCurrentItem() == "Treasure" && !doorUsed)
        {
            sp.GetInteractionUI()?.ShowReturn();

            if (Input.GetButtonDown("Fire1"))
            {
                photonView.RPC(
                    nameof(RPC_RequestTreasureDelivery),
                    RpcTarget.MasterClient,
                    sp.photonView.ViewID
                );
            }
            return;
        }

        // 2️⃣ Escape
        if (!doorUsed &&
            TreasureScoreManager.Instance.GetTreasureCount() >=
            TreasureScoreManager.Instance.totalTreasures)
        {
            sp.GetInteractionUI()?.ShowEnter();

            if (Input.GetKeyDown(KeyCode.E))
            {
                sp.photonView.RPC(
                    nameof(SmallPlayerItemCollector.RPC_Escape),
                    RpcTarget.All
                );
            }
            return;
        }

        sp.GetInteractionUI()?.HideAll();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SmallPlayerItemCollector sp = other.GetComponent<SmallPlayerItemCollector>();
        if (sp != null && sp.photonView.IsMine)
            sp.GetInteractionUI()?.HideAll();
    }

    // MASTER ONLY
    [PunRPC]
    private void RPC_RequestTreasureDelivery(int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient || doorUsed) return;

        doorUsed = true;

        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            playerPV.RPC(
                nameof(SmallPlayerItemCollector.RPC_OnTreasureDelivered),
                RpcTarget.All
            );
        }

        // Increase treasure count
        TreasureScoreManager.Instance.AddTreasure_Master();

        if (!doorClosed && door != null)
            photonView.RPC(nameof(RPC_CloseDoor), RpcTarget.All);

        // Only teleport treasure if total treasures not yet reached
        if (TreasureScoreManager.Instance.GetTreasureCount() <
            TreasureScoreManager.Instance.totalTreasures)
        {
            TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
            if (spawner != null)
                spawner.TeleportTreasure();
        }
    }


    [PunRPC]
    private void RPC_CloseDoor()
    {
        if (doorClosed) return;
        StartCoroutine(CloseDoor());
    }

    private IEnumerator CloseDoor()
    {
        doorClosed = true;

        Quaternion start = door.localRotation;
        Quaternion end = Quaternion.Euler(0f, doorCloseAngle, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * doorCloseSpeed;
            door.localRotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }

        door.localRotation = end;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(doorClosed);
            stream.SendNext(doorUsed);
        }
        else
        {
            doorClosed = (bool)stream.ReceiveNext();
            doorUsed = (bool)stream.ReceiveNext();
        }
    }
}

