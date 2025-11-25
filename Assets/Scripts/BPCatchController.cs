using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BPCatchController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string grabAnimation = "BPRig_BP_Catch"; // Catch animation
    [SerializeField] private int catchLayer = 1; // CatchAnim layer index

    [Header("Camera / Arm Settings")]
    public Transform playerCamera;
    [SerializeField] private string shoulderBoneName = "shoulder.l";
    [Range(0.01f, 1f)]
    public float aimBlend = 0.2f; // Smoothness of shoulder rotation
    public float cameraAimDistance = 10f;
    public float verticalOffset = 0.0f;

    [Header("Catch Cooldown")]
    public float catchCooldown = 1f; // time in seconds between catches
    private bool canCatch = true;

    private Animator animator;
    private PhotonView photonView;
    private Transform shoulderBone;

    private bool isGrabbing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        photonView = GetComponentInParent<PhotonView>();

        if (animator == null)
            Debug.LogError("Animator missing!");
        if (photonView == null)
            Debug.LogError("PhotonView not found!");

        shoulderBone = FindDeepChild(animator.transform, shoulderBoneName);
        if (shoulderBone == null)
            Debug.LogError($"Bone '{shoulderBoneName}' not found!");

        // Ensure CatchAnim layer starts disabled
        animator.SetLayerWeight(catchLayer, 0f);
    }

    void Update()
    {
        if (photonView != null && photonView.IsMine && Input.GetButtonDown("Fire1") && canCatch)
        {
            PlayCatchLocally();
            photonView.RPC(nameof(RPC_PlayCatch), RpcTarget.Others);

            // Start cooldown
            canCatch = false;
            Invoke(nameof(ResetCatchCooldown), catchCooldown);
        }
    }

    void LateUpdate()
    {
        if (shoulderBone == null || playerCamera == null) return;

        if (isGrabbing)
        {
            RotateShoulderTowardCamera();

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(catchLayer);
            if (state.IsName(grabAnimation) && state.normalizedTime >= 1f)
            {
                animator.SetLayerWeight(catchLayer, 0f); // instantly remove layer
                isGrabbing = false;
            }
        }
    }

    void PlayCatchLocally()
    {
        StartCoroutine(StartCatch());
    }

    private IEnumerator StartCatch()
    {
        // 1. Force layer weight to 1 immediately
        animator.SetLayerWeight(catchLayer, 1f);

        // 2. Wait one frame so Animator updates the layer
        yield return null;

        // 3. Play the catch animation immediately
        animator.Play(grabAnimation, catchLayer, 0f);

        // 4. Set grabbing flag
        isGrabbing = true;
    }

    private void ResetCatchCooldown()
    {
        canCatch = true;
    }

    [PunRPC]
    void RPC_PlayCatch()
    {
        PlayCatchLocally();
    }

    void RotateShoulderTowardCamera()
    {
        Vector3 targetPoint = playerCamera.position + playerCamera.forward * cameraAimDistance;
        targetPoint.y -= verticalOffset;

        Vector3 direction = targetPoint - shoulderBone.position;
        Quaternion targetWorldRot = Quaternion.LookRotation(direction, Vector3.up);

        Transform parent = shoulderBone.parent;
        if (parent != null)
        {
            Quaternion targetLocal = Quaternion.Inverse(parent.rotation) * targetWorldRot;
            shoulderBone.localRotation = Quaternion.Slerp(
                shoulderBone.localRotation,
                targetLocal,
                aimBlend
            );
        }
        else
        {
            shoulderBone.rotation = Quaternion.Slerp(
                shoulderBone.rotation,
                targetWorldRot,
                aimBlend
            );
        }
    }

    // Recursive search for a child by name
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
