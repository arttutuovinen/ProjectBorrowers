using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BigPlayerStun : MonoBehaviourPun
{
    public float stunDuration = 5f;
    private bool isStunned = false;
    private BigPlayerMovement bigPlayerMovement;

    void Start()
    {
        bigPlayerMovement = GetComponent<BigPlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StunItem"))
        {
            // Request the BigPlayer to stun themselves on their own client
            photonView.RPC(nameof(StunRPC), photonView.Owner);

            // Destroy the StunItem over network
            PhotonView itemPV = other.GetComponent<PhotonView>();
            if (itemPV != null && itemPV.IsMine)
                PhotonNetwork.Destroy(other.gameObject);
            else if (itemPV != null)
                photonView.RPC(nameof(RequestItemDestroyRPC), itemPV.Owner, itemPV.ViewID);
        }
    }

    [PunRPC]
    private void StunRPC()
    {
        if (!photonView.IsMine) return; // Only stun on the owner client
        if (isStunned) return;
        StartCoroutine(StunPlayer());
    }

    IEnumerator StunPlayer()
    {
        isStunned = true;

        if (bigPlayerMovement != null)
            bigPlayerMovement.enabled = false;

        yield return new WaitForSeconds(stunDuration);

        if (bigPlayerMovement != null)
            bigPlayerMovement.enabled = true;

        isStunned = false;
    }

    [PunRPC]
    private void RequestItemDestroyRPC(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (itemView != null && itemView.IsMine)
            PhotonNetwork.Destroy(itemView.gameObject);
    }
}

