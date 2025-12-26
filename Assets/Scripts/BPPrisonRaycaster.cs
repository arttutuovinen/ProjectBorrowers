using UnityEngine;
using Photon.Pun;

public class BPPrisonRaycaster : MonoBehaviour
{
    public float rayDistance = 6f;
    public LayerMask prisonLayer;
    public Transform bpCameraTransform;

    private PhotonView capturedSP;

    public void SetCapturedSP(PhotonView sp)
    {
        capturedSP = sp;
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E) || capturedSP == null) return;
        Debug.Log("attempting to jail SP");
        Ray ray = new Ray(bpCameraTransform.position, bpCameraTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, prisonLayer)) return;

        foreach (Transform t in hit.collider.GetComponentsInChildren<Transform>())
        {
            if (!t.CompareTag("SmallPlayerJailTeleport")) continue;

            capturedSP.RPC("RPC_OnJailed", RpcTarget.All, t.position);
            capturedSP = null;
            break;
        }
        BPFpAnimationController bpAnim = GetComponentInParent<BPFpAnimationController>();
        if (bpAnim != null && bpAnim.photonView.IsMine)
        {
            bpAnim.ResetCaughtAnimation();
        }
    }
}
