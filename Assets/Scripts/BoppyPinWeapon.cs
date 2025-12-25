using UnityEngine;
using Photon.Pun;

public class BoppyPinWeapon : MonoBehaviourPun
{
    private bool hasHit = false;

    private void Awake()
    {
        // Ensure correct physics setup
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent double hits (network + physics safety)
        if (hasHit) return;
        hasHit = true;

        // Only the weapon owner processes the hit
        if (!photonView.IsMine) return;

        if (!other.CompareTag("BigPlayer")) return;

        PhotonView bigPlayerPV = other.GetComponent<PhotonView>();
        if (bigPlayerPV == null) return;

        // Tell the Big Player to stun themselves
        bigPlayerPV.RPC("StunRPC", bigPlayerPV.Owner);

        // Destroy weapon network-wide (only owner can do this)
        PhotonNetwork.Destroy(gameObject);
    }
}
