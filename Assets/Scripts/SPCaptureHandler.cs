using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SPCaptureHandler : MonoBehaviourPun
{
    private CharacterController controller;
    private SPMovementNET movementScript;
    private Transform spFollowTarget;

    private GameObject spProxy_SP;
    private GameObject spProxy_BP;

    public Transform bpPrefabTransform;

    private bool isCaptured = false; // ✅ RESTORED
    private bool isJailed = false;

    private GameObject escapeBar;
    private Image escapePanel;
    private float escapeValue = 0f;
    private float escapeDecreaseSpeed = 0.92f;
    private float escapeIncreaseAmount = 0.15f;

    public static List<SPCaptureHandler> JailedSPs = new List<SPCaptureHandler>();

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<SPMovementNET>();
    }

    void Start()
    {
        CreateInvisibleProxies();

        if (photonView.IsMine)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            escapeBar = canvas.transform.Find("SmallPlayerUI/EscapeBar")?.gameObject;
            escapePanel = canvas.transform.Find("SmallPlayerUI/EscapeBar/Panel")?.GetComponent<UnityEngine.UI.Image>();

            if (escapeBar != null)
                escapeBar.SetActive(false);
        }
    }

    void Update()
    {
        if (!isCaptured || !photonView.IsMine || spFollowTarget == null)
            return;

        controller.enabled = false;
        transform.position = spFollowTarget.position;
        controller.enabled = true;

        HandleEscape();
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
            
            if (escapeBar != null)
            {
                escapeBar.SetActive(true);
                escapeValue = 0f;
                escapePanel.fillAmount = 0f;
            }
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
        isJailed = true;

        if (!JailedSPs.Contains(this))
            JailedSPs.Add(this);

        if (photonView.IsMine)
        {
            SPWinManager.Instance.photonView.RPC(
                nameof(SPWinManager.RPC_ReportCaptured),
                RpcTarget.MasterClient
            );
        }

        controller.enabled = false;
        transform.position = jailPos;
        controller.enabled = true;

        if (photonView.IsMine)
        {
            movementScript.EnableMovement();
            escapeBar.SetActive(false);
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

    public bool IsJailed()
    {
        return isJailed;
    }

    void HandleEscape()
    {
        if (escapePanel == null) return;

        // decrease bar
        escapeValue -= Time.deltaTime * escapeDecreaseSpeed;
        escapeValue = Mathf.Clamp01(escapeValue);

        // press interact to increase
        if (Input.GetButtonDown("P1Interact"))
        {
            escapeValue += escapeIncreaseAmount;
        }

        escapePanel.fillAmount = escapeValue;

        // escape success
        if (escapeValue >= 1f)
        {
            Escape();
        }
    }

    [PunRPC]
    void RPC_OnEscape()
    {
        SetRenderers(true);

        if (spProxy_BP != null)
            spProxy_BP.SetActive(false);
    }

    void Escape()
    {
        isCaptured = false;
        spFollowTarget = null;

        if (escapeBar != null)
            escapeBar.SetActive(false);

        controller.enabled = true;
        movementScript.EnableMovement();

        SetRenderers(true);
        photonView.RPC("RPC_OnEscape", RpcTarget.All);

        if (spProxy_SP != null) spProxy_SP.SetActive(false);

        photonView.RPC("RPC_HideBPProxy", RpcTarget.Others);

        BPFpAnimationController bpAnim = FindFirstObjectByType<BPFpAnimationController>();
        if (bpAnim != null)
        {
            bpAnim.photonView.RPC("RPC_ResetCaughtAnimation", bpAnim.photonView.Owner, null);
        }

        // stun BP
        BigPlayerStun bp = FindFirstObjectByType<BigPlayerStun>();
        if (bp != null)
        {
            bp.photonView.RPC("StunRPC", RpcTarget.All);
        }

        BPCatchController[] bpControllers = FindObjectsByType<BPCatchController>(FindObjectsSortMode.None);

        foreach (var bpController in bpControllers)
        {
            bpController.photonView.RPC("RPC_ResetCaughtReactionSP", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_FreeFromJail(Vector3 freePosition)
    {
        isJailed = false;
        isCaptured = false;
        spFollowTarget = null;

        if (JailedSPs.Contains(this))
            JailedSPs.Remove(this);

        controller.enabled = false;
        transform.position = freePosition;
        controller.enabled = true;

        SetRenderers(true);

        if (photonView.IsMine && movementScript != null)
            movementScript.EnableMovement();

        if (photonView.IsMine)
        {
            if (escapeBar != null)
                escapeBar.SetActive(false);

            if (escapePanel != null)
                escapePanel.fillAmount = 0f;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            BPScoreManager score = FindFirstObjectByType<BPScoreManager>();
            if (score != null)
            {
                PhotonView scorePV = score.GetComponent<PhotonView>();
                scorePV.RPC("RPC_ForceRemovePlayer", RpcTarget.All, photonView.ViewID);
            }
        }
    }
}
