using UnityEngine;
using Photon.Pun;

public class SPCaptureHandler : MonoBehaviourPun
{
    private CharacterController controller;
    private SPMovementNET movementScript;
    private Transform followTarget;
    private bool isCaptured = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<SPMovementNET>();
    }

    [PunRPC]
    public void RPC_CapturePlayer(int bpProxyTeleportID, int spProxyTeleportID)
    {
        if (!photonView.IsMine) return;

        // SP proxy target
        PhotonView spPV = PhotonView.Find(spProxyTeleportID);
        if (spPV == null) return;

        followTarget = spPV.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.DisableMovement();

        // Spawn SP client proxy
        SpawnSPProxy(followTarget);
    }

    private void SpawnSPProxy(Transform target)
    {
        GameObject proxyPrefab = Resources.Load<GameObject>("SmallPlayer Proxy");
        if (proxyPrefab == null) return;

        GameObject proxy = Instantiate(proxyPrefab, target.position, target.rotation);
        proxy.name = "SP_Proxy_SP";

        SPProxyFollower follower = proxy.AddComponent<SPProxyFollower>();
        follower.teleportTarget = target;
        follower.bpCamera = null;
    }

    private void Update()
    {
        if (!isCaptured || followTarget == null || !photonView.IsMine) return;

        controller.enabled = false;
        transform.position = Vector3.Lerp(
            transform.position,
            followTarget.position,
            Time.deltaTime * 20f
        );
        controller.enabled = true;
    }

    [PunRPC]
    public void RPC_HideSP()
    {
        // Disable all renderers to make SP invisible but keep GameObject active for camera
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
    }

    [PunRPC]
    public void RPC_PlayCaughtReaction()
    {
        // Play BP caught animation on SP client
        BPCatchController bpController = FindObjectOfType<BPCatchController>();
        if (bpController != null)
            bpController.PlayCaughtReaction();
    }

    public bool IsCaptured()
    {
        return isCaptured;
    }
}
