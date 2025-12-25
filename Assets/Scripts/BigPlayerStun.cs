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

