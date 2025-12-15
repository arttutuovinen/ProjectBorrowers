using UnityEngine;
using Photon.Pun;

public class BPPrisonRaycaster : MonoBehaviour
{
    public float rayDistance = 6f;
    public LayerMask prisonLayer; // assign Prison layer in Inspector
    private BPCatchCollider bpCatchCollider;
    public Transform bpCameraTransform;
    private PhotonView bpPhotonView; // local BP PhotonView

    private void Awake()
    {
        bpCatchCollider = GetComponent<BPCatchCollider>();
        bpPhotonView = GetComponent<PhotonView>(); // root’s PhotonView
    }

    private void Update()
    {
        if (bpPhotonView == null || !bpPhotonView.IsMine) return;
        if (bpCameraTransform == null)
        {
            Debug.LogWarning("BP Camera Transform not assigned!");
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (bpCatchCollider == null || bpCatchCollider.capturedSP == null) return;

        SPCaptureHandler sp = bpCatchCollider.capturedSP;
        PhotonView spPV = bpCatchCollider.capturedSPPhotonView;
        if (!sp.IsCaptured() || spPV == null) return;

        Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green, 1f);
        Debug.Log("Shooting ray from BP camera");

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Ray did not hit any Prison layer collider");
            return;
        }

        Debug.Log("Ray hit: " + hit.collider.name);

        Transform jailTeleport = null;
        foreach (Transform t in hit.collider.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("SmallPlayerJailTeleport"))
            {
                jailTeleport = t;
                break;
            }
        }

        if (jailTeleport == null) return;

        spPV.RPC("RPC_TeleportToJail", spPV.Owner, jailTeleport.position);
    }
}
