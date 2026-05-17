using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class SPIsStunned : MonoBehaviourPun
{
    public float stunDuration = 5f;    // Duration for which the player is stunned
    private bool isStunned = false;    // Tracks if the player is stunned
    private SPMovementNET smallPlayerMovement; // Reference to the player's movement script (assuming a separate movement script exists)
    private SPInteractionUI spInteraction;

    void Start()
    {
        smallPlayerMovement = GetComponent<SPMovementNET>();
        spInteraction = GetComponent<SPInteractionUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.CompareTag("MouseTrapWeapon") && !isStunned)
        {
            StartCoroutine(StunPlayer());

            PhotonView trapPV = other.GetComponent<PhotonView>();
            if (trapPV != null)
            {
                trapPV.RPC("RPC_DestroyTrap", trapPV.Owner);
            }
        }
    }

    // Coroutine to handle the stun logic
    IEnumerator StunPlayer()
    {
        // Set the player to stunned
        isStunned = true;
        spInteraction.ShowStunned();

        // Disable the player's movement
        if (smallPlayerMovement != null)
        {
            smallPlayerMovement.enabled = false;
        }

        // Wait for the stun duration
        yield return new WaitForSeconds(stunDuration);

        // Re-enable movement after stun is over
        if (smallPlayerMovement != null)
        {
            smallPlayerMovement.enabled = true;
        }
        spInteraction.HideAll();
        // Reset the stun state
        isStunned = false;
    }
}
