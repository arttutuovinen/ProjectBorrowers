using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public PhotonView teleportPV; // Drag the SPTeleportLocation PhotonView here

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;         // Only BP owner triggers
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null || teleportPV == null) return;

        Debug.Log("BP caught SP!");

        // Tell SP owner they are captured
        spPV.RPC("RPC_CapturePlayer", RpcTarget.All, teleportPV.ViewID);

        // Spawn proxy locally on BP client
        SpawnProxy(spPV);
    }

    private void SpawnProxy(PhotonView spPV)
    {
        GameObject realSP = spPV.gameObject;

        // Hide the real SP model ONLY on BP client
        Renderer[] renderers = realSP.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        // Spawn a local-only clone
        GameObject proxy = Instantiate(realSP);
        proxy.name = "SP_Proxy";

        // Remove any PhotonView from the proxy (local object)
        PhotonView proxyPV = proxy.GetComponent<PhotonView>();
        if (proxyPV != null)
            Destroy(proxyPV);

        // Ensure all Renderers are visible on the proxy
        Renderer[] proxyRenderers = proxy.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in proxyRenderers)
            r.enabled = true;

        // Attach follower script to proxy
        SPProxyFollower follower = proxy.AddComponent<SPProxyFollower>();
        follower.teleportTarget = teleportPV.transform;
    }
}
