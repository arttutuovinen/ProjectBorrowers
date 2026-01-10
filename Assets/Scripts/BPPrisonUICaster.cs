using UnityEngine;
using Photon.Pun;

public class BPPrisonUICaster : MonoBehaviour
{
    public float rayDistance = 6f;
    public LayerMask prisonLayer;
    public Transform bpCameraTransform;

    private SPCaptureHandler spCaptureHandler;
    private BPInteractionUI interactionUI;

    void Start()
    {
        spCaptureHandler = GetComponent<SPCaptureHandler>();
        interactionUI = GetComponentInParent<BPInteractionUI>();
    }

    void Update()
    {
        if (interactionUI == null) return;

        bool showUI = false;

        // Only if BP has captured a SP
        if (spCaptureHandler != null && spCaptureHandler.IsCaptured())
        {
            Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer))
            {
                showUI = true;
            }
        }

        if (showUI)
            interactionUI.ShowCaptureSP();
        else
            interactionUI.HideAll();
    }
}
