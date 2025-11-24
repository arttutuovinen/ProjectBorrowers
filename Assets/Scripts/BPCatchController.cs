using UnityEngine;
using Photon.Pun;

public class BPCatchController : MonoBehaviour
{
    private Animator animator;
    private PhotonView parentPhotonView;

    [SerializeField] private string grabAnimation = "BPRig_BP_Catch";

    // Assign your BigPlayerCamera transform in the inspector
    public Transform playerCamera;

    // bone name to find
    [SerializeField] private string shoulderBoneName = "shoulder.l";

    private Transform shoulderBone;
    private bool isGrabbing = false;

    // blending speed for rotation (0 - no change, 1 - instant)
    [Range(0.01f, 1f)]
    public float aimBlend = 0.2f;

    // distance ahead of camera to aim at
    public float cameraAimDistance = 10f;

    // vertical offset in meters (positive = lower, negative = higher)
    public float verticalOffset = 0.0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        parentPhotonView = GetComponentInParent<PhotonView>();

        if (animator == null)
            Debug.LogError("Animator missing on this GameObject.");

        if (parentPhotonView == null)
            Debug.LogError("No PhotonView found on parent.");

        // find shoulder bone anywhere under the animator's transform
        shoulderBone = FindDeepChild(animator.transform, shoulderBoneName);
        if (shoulderBone == null)
            Debug.LogError($"Bone '{shoulderBoneName}' not found in rig hierarchy! Check spelling and case.");

        if (playerCamera == null)
            Debug.LogWarning("playerCamera not assigned. Assign BigPlayerCamera in inspector.");
    }

    void Update()
    {
        if (parentPhotonView != null && parentPhotonView.IsMine && Input.GetButtonDown("Fire1"))
        {
            PlayGrabLocally();
            parentPhotonView.RPC(nameof(RPC_PlayGrab), RpcTarget.Others);
        }
    }

    void LateUpdate()
    {
        if (!isGrabbing || shoulderBone == null || playerCamera == null) return;

        RotateShoulderTowardCamera();
    }

    void PlayGrabLocally()
    {
        isGrabbing = true;

        // Layer 1 is assumed to be your masked Catch/Grab layer
        animator.CrossFade(grabAnimation, 0.05f, 1, 0f);

        // schedule stop when animation on layer 1 finishes
        var state = animator.GetCurrentAnimatorStateInfo(1);
        float length = state.length;
        if (length <= 0f) length = 1f; // fallback if state info not ready
        CancelInvoke(nameof(StopGrab));
        Invoke(nameof(StopGrab), length);
    }

    void StopGrab()
    {
        isGrabbing = false;
    }

    [PunRPC]
    void RPC_PlayGrab()
    {
        PlayGrabLocally();
    }

    // -----------------------------------------------------
    // Rotate shoulder.l to aim toward a point in camera view with vertical offset
    // -----------------------------------------------------
    void RotateShoulderTowardCamera()
    {
        // Base target point in front of camera
        Vector3 targetPoint = playerCamera.position + playerCamera.forward * cameraAimDistance;

        // Apply vertical offset (lower = positive, higher = negative)
        targetPoint.y -= verticalOffset;

        // Direction vector from shoulder to target
        Vector3 direction = targetPoint - shoulderBone.position;

        // World rotation needed to point shoulder toward target
        Quaternion targetWorldRot = Quaternion.LookRotation(direction, Vector3.up);

        Transform parent = shoulderBone.parent;
        if (parent == null)
        {
            shoulderBone.rotation = Quaternion.Slerp(
                shoulderBone.rotation,
                targetWorldRot,
                aimBlend
            );
            return;
        }

        // Convert to local rotation
        Quaternion targetLocal = Quaternion.Inverse(parent.rotation) * targetWorldRot;

        // Smooth blend
        shoulderBone.localRotation = Quaternion.Slerp(
            shoulderBone.localRotation,
            targetLocal,
            aimBlend
        );
    }

    // Recursive search to find a child by name anywhere in hierarchy
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
