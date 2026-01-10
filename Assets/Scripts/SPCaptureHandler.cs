using UnityEngine;
using Photon.Pun;
using System.Linq;

public class SPCaptureHandler : MonoBehaviourPun
{
    private CharacterController controller;
    private SPMovementNET movementScript;
    private Transform spFollowTarget;

    private GameObject spProxy_SP;
    private GameObject spProxy_BP;

    public Transform bpPrefabTransform;

    private bool isCaptured = false; // ✅ RESTORED

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<SPMovementNET>();
    }

    void Start()
    {
        CreateInvisibleProxies();
    }

    void Update()
    {
        if (!isCaptured || !photonView.IsMine || spFollowTarget == null)
            return;

        controller.enabled = false;
        transform.position = spFollowTarget.position;
        controller.enabled = true;
    }

    void CreateInvisibleProxies()
    {
        GameObject prefab = Resources.Load<GameObject>("SmallPlayerProxy");
        if (!prefab) return;

        spProxy_SP = Instantiate(prefab);
        spProxy_SP.name = "SP_Proxy_SP";
        spProxy_SP.tag = "SPProxy";
        spProxy_SP.SetActive(false);

        spProxy_BP = Instantiate(prefab);
        spProxy_BP.name = "SP_Proxy_BP";
        spProxy_BP.tag = "SPProxy";
        spProxy_BP.SetActive(false);
    }

    [PunRPC]
    public void RPC_AssignTeleportTargets(int bpTeleportID, int spTeleportID)
    {
        PhotonView bpTP = PhotonView.Find(bpTeleportID);
        PhotonView spTP = PhotonView.Find(spTeleportID);
        if (bpTP == null || spTP == null) return;

        if (photonView.IsMine)
        {
            spFollowTarget = spTP.transform;

            if (!spProxy_SP.TryGetComponent(out SPClientProxyFollower spFollow))
                spFollow = spProxy_SP.AddComponent<SPClientProxyFollower>();

            spFollow.teleportTarget = spTP.transform;
            spFollow.bpTransform = bpPrefabTransform;
        }
        else
        {
            if (!spProxy_BP.TryGetComponent(out SPProxyFollower bpFollow))
                bpFollow = spProxy_BP.AddComponent<SPProxyFollower>();

            bpFollow.teleportTarget = bpTP.transform;
        }
    }

    [PunRPC]
    public void RPC_OnCaptured()
    {
        isCaptured = true;

        if (photonView.IsMine)
        {
            movementScript.DisableMovement();
            SetRenderers(false); // hide real SP
            spProxy_SP.SetActive(true); // show SP proxy following SPClientTeleportLocation
        }
        else
        {
            SetRenderers(false); // hide SP prefab for BP
            spProxy_BP.SetActive(true); // show BP-side proxy following SmallPlayerTeleportPosition
        }
    }

    [PunRPC]
    public void RPC_OnJailed(Vector3 jailPos)
    {
        spFollowTarget = null;
        isCaptured = false;

        controller.enabled = false;
        transform.position = jailPos;
        controller.enabled = true;

        if (photonView.IsMine)
        {
            movementScript.EnableMovement();
            
        }
        SetRenderers(true); // show SP prefab
        // Reset SP proxies on all clients
        if (spProxy_SP != null) spProxy_SP.SetActive(false);
        if (spProxy_BP != null && !photonView.IsMine)
            spProxy_BP.SetActive(false);

        // Reset BP caught animation on SP client
        BPCatchController[] bpControllers =
    FindObjectsByType<BPCatchController>(FindObjectsSortMode.None);
        foreach (var bp in bpControllers)
        {
            if (!bp.photonView.IsMine)
                bp.photonView.RPC("RPC_ResetCaughtReactionSP", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_HideBPProxy()
    {
        if (spProxy_BP != null)
            spProxy_BP.SetActive(false);
    }

    void SetRenderers(bool state)
    {
        // Regular Mesh Renderers
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            r.enabled = state;

        // Skinned Mesh Renderers
        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            smr.enabled = state;
    }

    // ✅ REQUIRED BY SPMovementNET
    public bool IsCaptured()
    {
        return isCaptured;
    }
}
