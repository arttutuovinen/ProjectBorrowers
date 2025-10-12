using UnityEngine;
using System.Collections;

public class BPMedicine : MonoBehaviour
{
    public BigPlayerMovement playerMovement;
    public float boostDuration = 3f;
    public float boostedSpeed = 15f;
    private float originalSpeed;
    private Coroutine boostCoroutine;

    public void ActivateSpeedBoost()
    {
        boostCoroutine = StartCoroutine(SpeedBoostRoutine());
    }
    private IEnumerator SpeedBoostRoutine()
    {
        originalSpeed = playerMovement.currentSpeed;
        playerMovement.currentSpeed = boostedSpeed;
        yield return new WaitForSeconds(boostDuration);
        playerMovement.currentSpeed = originalSpeed;
        boostCoroutine = null;
    }
}
