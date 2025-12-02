using UnityEngine;
using Photon.Pun;

public class BPFpAnimationController : MonoBehaviourPun
{
    [Header("References")]
    public Animator fpAnimator;   // Animator on BP_fpModel

    [Header("Settings")]
    public float catchCooldown = 1.5f;

    private bool isCoolingDown = false;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetButtonDown("Fire1"))
        {
            TryCatch();
        }
    }

    void TryCatch()
    {
        if (isCoolingDown) return;

        StartCoroutine(CatchRoutine());
    }

    System.Collections.IEnumerator CatchRoutine()
    {
        isCoolingDown = true;

        // Trigger the Catch animation
        fpAnimator.SetBool("IsCatching", true);

        // Allow the Animator to manage the animation via transitions
        yield return null;
        fpAnimator.SetBool("IsCatching", false);

        // Start cooldown
        yield return new WaitForSeconds(catchCooldown);

        isCoolingDown = false;
    }
}
