using UnityEngine;
using System.Collections;
using Photon.Pun;

public class BPMedicine : MonoBehaviourPun
{
    public BigPlayerMovement playerMovement;
    public float boostDuration = 3f;
    public float boostedSpeed = 15f;

    private float originalSpeed;
    private Coroutine boostCoroutine;

    public void ActivateSpeedBoost()
    {
        if (!photonView.IsMine) return;   // ✔ Only local player can trigger

        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(SpeedBoostRoutine());
        photonView.RPC("RPC_PlayBoostEffect", RpcTarget.Others); // ✔ Sync visual effect (optional)
    }

    private IEnumerator SpeedBoostRoutine()
    {
        originalSpeed = playerMovement.currentSpeed;
        playerMovement.currentSpeed = boostedSpeed;

        yield return new WaitForSeconds(boostDuration);

        playerMovement.currentSpeed = originalSpeed;
        boostCoroutine = null;
    }

    [PunRPC]
    void RPC_PlayBoostEffect()
    {
        // Optional: put VFX/SFX for other players here
        // Does NOT affect speed on remote clients
    }
}

