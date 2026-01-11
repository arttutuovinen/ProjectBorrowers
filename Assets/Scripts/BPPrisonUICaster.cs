using UnityEngine;
using Photon.Pun;

public class BPPrisonUICaster : MonoBehaviour
{
    public float rayDistance = 6f;
    public LayerMask prisonLayer;
    public Transform bpCameraTransform;

    private SPCaptureHandler spCaptureHandler;
    private BPInteractionUI interactionUI;
    private PhotonView ownerPV;

    void Start()
    {
        ownerPV = GetComponentInParent<PhotonView>();
        spCaptureHandler = GetComponent<SPCaptureHandler>();
        interactionUI = GetComponentInParent<BPInteractionUI>();
    }

    void Update()
    {
        // Only run for the local BP player
        if (ownerPV == null || !ownerPV.IsMine) return;
        if (interactionUI == null || bpCameraTransform == null) return;

        bool showUI = false;

        // Only check raycast if BP has captured a SP
        if (spCaptureHandler != null && spCaptureHandler.IsCaptured())
        {
            Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);

            // Draw the ray in Scene view
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);

            // Raycast against the prisonLayer, include triggers
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer, QueryTriggerInteraction.Collide))
            {
                showUI = true;
                Debug.Log("Ray hit prison trigger: " + hit.collider.name);
            }
        }

        // Show or hide the CaptureSP UI
        if (showUI)
            interactionUI.ShowCaptureSP();
        else
            interactionUI.HideAll();
    }
}

