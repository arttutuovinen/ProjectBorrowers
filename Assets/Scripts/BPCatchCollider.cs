using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviour
{
    public PhotonView bpTeleportPV;
    public PhotonView spClientTeleportPV;
    public Camera bpCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null) return;
        
        SPCaptureHandler spHandler = other.GetComponent<SPCaptureHandler>();
        if (spHandler != null && spHandler.IsJailed())
        {
            // ❌ Already in jail → ignore
            return;
        }

        SPCaptureHandler[] allSP = FindObjectsByType<SPCaptureHandler>(FindObjectsSortMode.None);

        foreach (var sp in allSP)
        {
            if (sp != spHandler && sp.IsCaptured())
                return;
        }

        // Assign teleport targets
        spPV.RPC(
            "RPC_AssignTeleportTargets",
            RpcTarget.All,
            bpTeleportPV.ViewID,
            spClientTeleportPV.ViewID
        );

        // Capture SP
        spPV.RPC("RPC_OnCaptured", RpcTarget.All);
        spPV.RPC("RPC_DropTreasureOnCapture", spPV.Owner);

        BPScoreManager pm = FindFirstObjectByType<BPScoreManager>();
        pm.photonView.RPC("RPC_DisableScoreTrigger", RpcTarget.All);
        // ✅ Play caught animation on BP client
        BPFpAnimationController bpAnim = GetComponentInParent<BPFpAnimationController>();
        if (bpAnim != null && bpAnim.photonView.IsMine)
        {
            bpAnim.PlayCaughtAnimation();
        }

        // On BP client only
        BPCatchController bpCatchController = GetComponentInParent<BPCatchController>();
        if (bpCatchController != null)
        {
            // Call RPC to all other clients except BP client
            bpCatchController.photonView.RPC("RPC_PlayCaughtReactionSP", RpcTarget.Others);
        }
        // After capturing SP
        BPPrisonRaycaster prisonRaycaster = GetComponentInParent<BPPrisonRaycaster>();
        if (prisonRaycaster != null)
        {
            prisonRaycaster.SetCapturedSP(spPV);
        }
    }

}
