using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public PhotonView teleportPV; // Drag the SPTeleportLocation PhotonView here
    public PhotonView spClientTeleportPV; //SPClientTeleportLocation PhotonView
    public Camera bpCamera;       // Assign BP's camera in Inspector
    public BPFpAnimationController bpAnimation;

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null || teleportPV == null || spClientTeleportPV == null) return;

        Debug.Log("BP caught SP!");

        // Make SP invisible on both clients
        spPV.RPC("RPC_HideSP", RpcTarget.All);

        // Tell SP client it is captured and should spawn its own proxy
        spPV.RPC("RPC_CapturePlayer", RpcTarget.All,
            teleportPV.ViewID,
            spClientTeleportPV.ViewID);

        // Spawn BP client proxy
        SpawnBPProxy();

        // Play BP caught animation locally
        if (bpAnimation != null)
            bpAnimation.PlayCaughtAnimation();

        // Trigger caught animation on SP client
        spPV.RPC("RPC_PlayCaughtReaction", RpcTarget.Others);
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
