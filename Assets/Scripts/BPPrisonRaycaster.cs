using UnityEngine;
using Photon.Pun;
using System.Linq;

public class BPPrisonRaycaster : MonoBehaviour
{
    [Header("Ray Settings")]
    public float rayDistance = 6f;
    public LayerMask prisonLayer;
    public Transform bpCameraTransform;

    [Header("UI")]
    public BPInteractionUI interactionUI;

    private PhotonView capturedSP;        // SP currently held by BP
    private PhotonView ownerPV;

    void Start()
    {
        ownerPV = GetComponent<PhotonView>();
        if (bpCameraTransform == null)
            Debug.LogWarning("BPPrisonRaycaster: BP Camera not assigned!");
        if (prisonLayer.value == 0)
            Debug.LogWarning("BPPrisonRaycaster: Prison LayerMask not assigned!");
    }

    /// <summary>
    /// Called when BP captures a Small Player
    /// </summary>
    public void SetCapturedSP(PhotonView sp)
    {
        capturedSP = sp;
    }

    void Update()
    {
        // Only local Big Player runs this
        if (ownerPV == null || !ownerPV.IsMine) return;
        if (bpCameraTransform == null || interactionUI == null) return;

        // Draw debug ray
        Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);

        // Show CaptureSP UI if BP has a captured SP and ray hits prison trigger
        if (capturedSP != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer, QueryTriggerInteraction.Collide))
            {
                interactionUI.ShowCaptureSP();
                Debug.Log("Ray hit prison trigger: " + hit.collider.name);

                // Handle E press to jail SP
                if (Input.GetKeyDown(KeyCode.E))
                {
                    JailSP(hit);
                }
            }
        }
    }

    private void JailSP(RaycastHit hit)
    {
        if (capturedSP == null) return;

        foreach (Transform t in hit.collider.GetComponentsInChildren<Transform>())
        {
            if (!t.CompareTag("SmallPlayerJailTeleport")) continue;

            // Teleport SP to jail
            capturedSP.RPC("RPC_OnJailed", RpcTarget.All, t.position);

            // Notify score manager
            BPScoreManager pm = FindFirstObjectByType<BPScoreManager>();
            pm.photonView.RPC("RPC_EnableScoreTrigger", RpcTarget.All);

            // Clear captured SP reference
            capturedSP = null;
            break;
        }

        // Reset BP animations
        BPFpAnimationController bpAnim = GetComponentInParent<BPFpAnimationController>();
        if (bpAnim != null && bpAnim.photonView.IsMine)
        {
            bpAnim.ResetCaughtAnimation();
        }
    }
}
