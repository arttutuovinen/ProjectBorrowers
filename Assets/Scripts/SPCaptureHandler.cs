using UnityEngine;
using Photon.Pun;

public class SPCaptureHandler : MonoBehaviourPun
{
    private CharacterController controller;
    private SPMovementNET movementScript;
    private Transform followTarget;
    private bool isCaptured = false;
    public Transform bpPrefabTransform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<SPMovementNET>();
    }

    [PunRPC]
    public void RPC_CapturePlayer(int bpProxyTeleportID, int spProxyTeleportID)
    {
        if (!photonView.IsMine) return; // Only SP client runs this

        // SP client teleport target
        PhotonView spTeleportPV = PhotonView.Find(spProxyTeleportID);
        if (spTeleportPV == null) return;

        followTarget = spTeleportPV.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.DisableMovement();

        // Spawn SP client proxy locally (SP client only)
        SpawnSPProxy(followTarget);
    }

    private void SpawnSPProxy(Transform target)
    {
        GameObject proxyPrefab = Resources.Load<GameObject>("SmallPlayerProxy");
        if (proxyPrefab == null) return;

        GameObject proxy = Instantiate(proxyPrefab, target.position, Quaternion.identity);
        proxy.name = "SP_Proxy_SP";

        SPClientProxyFollower follower = proxy.AddComponent<SPClientProxyFollower>();
        follower.teleportTarget = target;
        follower.bpTransform = bpPrefabTransform; // SP proxy looks at BP prefab
        follower.yRotationOffset = 0f;
    }

    private void Update()
    {
        if (!isCaptured || followTarget == null || !photonView.IsMine) return;

        controller.enabled = false;
        transform.position = Vector3.Lerp(transform.position, followTarget.position, Time.deltaTime * 20f);
        controller.enabled = true;
    }

    [PunRPC]
    public void RPC_HideSP()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    [PunRPC]
    public void RPC_PlayCaughtReaction()
    {
        BPCatchController bpController = FindObjectOfType<BPCatchController>();
        if (bpController != null)
            bpController.PlayCaughtReaction();
    }

    public bool IsCaptured()
    {
        return isCaptured;
    }
}
