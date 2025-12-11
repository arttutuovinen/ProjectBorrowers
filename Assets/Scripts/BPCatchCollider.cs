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

        if (bpAnimation != null)
            bpAnimation.PlayCaughtAnimation();

        PhotonView spPV = other.GetComponent<PhotonView>();
        if (spPV == null || teleportPV == null || spClientTeleportPV == null) return;

        Debug.Log("BP caught SP!");

        // Tell SP owner they are captured → send teleport locations
        spPV.RPC("RPC_CapturePlayer", RpcTarget.All,
            teleportPV.ViewID,                // proxy follows this
            spClientTeleportPV.ViewID);       // SP client follows this

        // Spawn proxy for BP client and SP client locally
        SpawnProxy(spPV);
    }

    private void SpawnProxy(PhotonView spPV)
    {
        GameObject realSP = spPV.gameObject;

        // Hide the real SP model ONLY on BP and SP clients
        Renderer[] renderers = realSP.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        // Hide SmallPlayerMesh if it exists
        Transform mesh = realSP.transform.Find("SmallPlayerMesh");
        if (mesh != null)
            mesh.gameObject.SetActive(false);

        // Spawn a local-only clone (proxy)
        GameObject proxy = Instantiate(realSP);
        proxy.name = "SP_Proxy";

        PhotonView proxyPV = proxy.GetComponent<PhotonView>();
        if (proxyPV != null)
        {
            proxyPV.enabled = false;
            proxyPV.ObservedComponents.Clear();
        }

        foreach (Renderer r in proxy.GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Attach follower script → follows SmallPlayerTeleportLocation
        SPProxyFollower follower = proxy.AddComponent<SPProxyFollower>();
        follower.teleportTarget = teleportPV.transform;
        follower.bpCamera = bpCamera;
    }
}
