using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public PhotonView teleportPV; // Drag the SPTeleportLocation PhotonView here
    public PhotonView spClientTeleportPV; //SPClientTeleportLocation PhotonView
    public Camera bpCamera;       // Assign BP's camera in Inspector
    public BPFpAnimationController bpAnimation;
    public BPCatchController bpSPClientanimation;

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (!other.CompareTag("SmallPlayer")) return;

        if (bpAnimation != null)
            bpAnimation.PlayCaughtAnimation();
        if (bpSPClientanimation != null)
            bpSPClientanimation.PlayCaughtReaction();

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null || teleportPV == null || spClientTeleportPV == null) return;

        Debug.Log("BP caught SP!");

        // Tell SP client it is captured
        spPV.RPC("RPC_CapturePlayer", RpcTarget.All,
            teleportPV.ViewID,       // BP proxy target
            spClientTeleportPV.ViewID); // SP proxy target

        // Spawn BP client proxy
        SpawnBPProxy();
    }

    private void SpawnBPProxy()
    {
        GameObject proxyPrefab = Resources.Load<GameObject>("SmallPlayerProxy");
        if (proxyPrefab == null) return;

        GameObject proxy = Instantiate(proxyPrefab, teleportPV.transform.position, teleportPV.transform.rotation);
        proxy.name = "SP_Proxy_BP";

        SPProxyFollower follower = proxy.AddComponent<SPProxyFollower>();
        follower.teleportTarget = teleportPV.transform;
        follower.bpCamera = bpCamera;
    }
}
