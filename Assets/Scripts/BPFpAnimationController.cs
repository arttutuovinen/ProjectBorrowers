using UnityEngine;
using Photon.Pun;

public class BPFpAnimationController : MonoBehaviourPun
{
    [Header("References")]
    public Animator fpAnimator;   // Animator on BP_fpModel
    public GameObject catchCollider;

    [Header("Settings")]
    public float catchCooldown = 1.5f;
    private bool isCoolingDown = false;

    public void Start()
    {
        catchCollider.SetActive(false);
        Debug.Log("CatchCOllider is deactivated");
    }

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

        // Enable collider
        catchCollider.SetActive(true);
        Debug.Log("catchCollider is Active");
        // Play animation
        fpAnimator.SetBool("IsCatching", true);

        // Wait for the animation clip length
        float clipLength = fpAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        // Animation done → stop animation + disable collider
        fpAnimator.SetBool("IsCatching", false);
        catchCollider.SetActive(false);
        Debug.Log("catchCollider is Deactivated");
        // Cooldown
        yield return new WaitForSeconds(catchCooldown);
        isCoolingDown = false;
    }
    public void PlayCaughtAnimation()
    {
        fpAnimator.SetBool("IsCaught", true);
    }

}
