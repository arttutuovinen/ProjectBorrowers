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

    void OnTriggerEnter(Collider collider)
    {
        if (!photonView.IsMine) return;

        if (!collider.CompareTag("StunItem")) return;
        Debug.Log("BP hit the boppyPIN");

        PhotonView weaponPV = collider.GetComponentInParent<PhotonView>();
        if (weaponPV == null) return;

        StunRPC();
        weaponPV.RPC("DestroyWeaponRPC", weaponPV.Owner);
    }

    [PunRPC]
    private void StunRPC()
    {
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
}

