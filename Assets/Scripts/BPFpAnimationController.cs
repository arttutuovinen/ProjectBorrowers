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

    // Track if any SP is currently captured
    private bool spCaptured = false;

    private BPItemCollector itemCollector;

    public void Start()
    {
        itemCollector = GetComponentInParent<BPItemCollector>();
        catchCollider.SetActive(false);
        Debug.Log("CatchCOllider is deactivated");
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // ❗ Block catch if BP is holding an item
        if (itemCollector != null && itemCollector.HasItem())
            return;

        if (Input.GetButtonDown("Fire1"))
        {
            TryCatch();
        }
    }

    void TryCatch()
    {
        if (itemCollector != null && itemCollector.IsUsingItem) return;
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
    // Called by BPCatchCollider when BP catches SP
    public void PlayCaughtAnimation()
    {
        fpAnimator.SetBool("IsCaught", true);
        spCaptured = true;         // ✅ Disable further catching
        catchCollider.SetActive(false); // ✅ Immediately disable catch collider
    }
    // Called when SP is teleported to jail
    public void ResetCaughtAnimation()
    {
        fpAnimator.SetBool("IsCaught", false);
        spCaptured = false;        // ✅ Allow catching again
    }
    [PunRPC]
    public void RPC_ResetCaughtAnimation()
    {
        ResetCaughtAnimation();
    }

    public void PlayCompassAnimation()
    {
        fpAnimator.SetBool("IsCompass", true);
    }
    public void ResetCompassAnimation()
    {
        fpAnimator.SetBool("IsCompass", false);
    }
    public void PlayVacuumAnimation()
    {
        fpAnimator.SetBool("IsVacuum", true);
    }
    public void ResetVacuumAnimation()
    {
        fpAnimator.SetBool("IsVacuum", false);
    }
}
