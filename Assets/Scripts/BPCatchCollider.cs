using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviour
{
    public PhotonView teleportPV; // Drag the SPTeleportLocation PhotonView here
    public PhotonView spClientTeleportPV; //SPClientTeleportLocation PhotonView
    public Camera bpCamera;       // Assign BP's camera in Inspector
    public BPFpAnimationController bpAnimation;

    // Store reference to currently captured SP
    [HideInInspector] public SPCaptureHandler capturedSP;
    [HideInInspector] public PhotonView capturedSPPhotonView;

    private void OnTriggerEnter(Collider other)
    {
        
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null || teleportPV == null || spClientTeleportPV == null) return;

        Debug.Log("BP caught SP!");

        // Store references for BPPrisonRaycaster
        capturedSP = spPV.GetComponent<SPCaptureHandler>();
        capturedSPPhotonView = spPV;

        // Hide SP mesh on all clients
        spPV.RPC("RPC_HideSP", RpcTarget.All);

        // Tell SP client to spawn its SP proxy locally
        spPV.RPC(
            "RPC_CapturePlayer",
            spPV.Owner,
            teleportPV.ViewID,
            spClientTeleportPV.ViewID
        );

        // Spawn BP client proxy locally (BP client only)
        SpawnBPProxy();

        // Play BP caught animation locally
        if (bpAnimation != null)
            bpAnimation.PlayCaughtAnimation();

        // Trigger SP caught animation on SP client
        spPV.RPC("RPC_PlayCaughtReaction", spPV.Owner);
    }

    private void SpawnBPProxy()
    {
        GameObject proxyPrefab = Resources.Load<GameObject>("SmallPlayerProxy");
        if (proxyPrefab == null) return;

        GameObject proxy = Instantiate(proxyPrefab, teleportPV.transform.position, teleportPV.transform.rotation);
        proxy.tag = "SPProxy";
        proxy.name = "SP_Proxy_BP";

        SPProxyFollower follower = proxy.AddComponent<SPProxyFollower>();
        follower.teleportTarget = teleportPV.transform;
        follower.bpCamera = bpCamera;
    }
}
