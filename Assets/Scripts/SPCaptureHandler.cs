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

        movementScript.DisableMovement();

        // Spawn SP client proxy locally
        SpawnSPProxy(followTarget);
    }

    private void SpawnSPProxy(Transform target)
    {
        GameObject prefab = Resources.Load<GameObject>("SmallPlayerProxy");
        if (!prefab) return;

        GameObject proxy = Instantiate(prefab, target.position, Quaternion.identity);
        proxy.tag = "SPProxy";
        proxy.name = "SP_Proxy_SP";

        SPClientProxyFollower follower = proxy.AddComponent<SPClientProxyFollower>();
        follower.teleportTarget = target;
        follower.bpTransform = bpPrefabTransform; // Faces BP prefab
    }

    private void Update()
    {
        if (!isCaptured || followTarget == null || !photonView.IsMine) return;

        controller.enabled = false;
        transform.position = followTarget.position;
        controller.enabled = true;
    }

    [PunRPC]
    public void RPC_TeleportToJail(Vector3 jailPos)
    {
        if (!photonView.IsMine) return;

        isCaptured = false;
        followTarget = null;

        controller.enabled = false;
        transform.position = jailPos;
        controller.enabled = true;

        // Re-enable movement
        movementScript.EnableMovement();

        // Re-enable renderers
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Destroy all SP proxies
        DestroyAllSPProxies();

        BPCatchController bpController = FindObjectOfType<BPCatchController>();
        if (bpController != null)
            bpController.ResetCaughtReaction();
    }

    private void DestroyAllSPProxies()
    {
        GameObject[] proxies = GameObject.FindGameObjectsWithTag("SPProxy");
        foreach (GameObject p in proxies)
            Destroy(p);
    }

    [PunRPC]
    public void RPC_HideSP()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
    [PunRPC]
    public void RPC_ShowSP()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
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
