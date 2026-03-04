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

    private bool hasJailed = false;

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
        hasJailed = false;
    }

    void Update()
    {
        if (ownerPV == null || !ownerPV.IsMine) return;
        if (bpCameraTransform == null || interactionUI == null) return;

        Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);

        if (capturedSP != null &&
            Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer, QueryTriggerInteraction.Collide))
        {
            interactionUI.ShowCaptureSP();
            Debug.Log("Ray hit prison trigger: " + hit.collider.name);

            if (Input.GetKeyDown(KeyCode.E))
            {
                JailSP(hit);
            }
        }
        else
        {
            // If ANY condition fails, release capture UI
            interactionUI.captureModeActive = false;
            interactionUI.HideCaptureSP();
        }
    }


    private void JailSP(RaycastHit hit)
    {
        if (capturedSP == null || hasJailed) return;

        hasJailed = true;

        Transform target = hit.collider.GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.CompareTag("SmallPlayerJailTeleport"));

        if (target == null)
        {
            hasJailed = false;
            return;
        }

        capturedSP.RPC("RPC_OnJailed", RpcTarget.All, target.position);

        BPScoreManager pm = FindFirstObjectByType<BPScoreManager>();
        if (pm != null)
        {
            pm.photonView.RPC("RPC_RequestAddCapture", RpcTarget.MasterClient);
            pm.photonView.RPC("RPC_EnableScoreTrigger", RpcTarget.All);
        }

        capturedSP = null;

        // Reset BP animations
        BPFpAnimationController bpAnim = GetComponentInParent<BPFpAnimationController>();
        if (bpAnim != null && bpAnim.photonView.IsMine)
        {
            bpAnim.ResetCaughtAnimation();
        }
    }
}
