using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BigPlayerStun : MonoBehaviourPun
{
    public float stunDuration = 4f;
    public float stunDurationSPCapture = 7f;
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
    public void StunRPC()
    {
        if (isStunned) return;
        StartCoroutine(StunPlayer());
    }

    [PunRPC]
    public void StunnedfromCaptureRPC()
    {
        if (isStunned) return;
        StartCoroutine(StunPlayerLong());
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

    IEnumerator StunPlayerLong()
    {
        isStunned = true;

        if (bigPlayerMovement != null)
            bigPlayerMovement.enabled = false;

        yield return new WaitForSeconds(stunDurationSPCapture);

        if (bigPlayerMovement != null)
            bigPlayerMovement.enabled = true;

        isStunned = false;
    }
}

