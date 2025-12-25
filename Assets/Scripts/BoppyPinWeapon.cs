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
    [PunRPC]
    public void DestroyWeaponRPC()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
