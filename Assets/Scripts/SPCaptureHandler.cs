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

        // Hide real SP mesh
        Transform mesh = transform.Find("SmallPlayerMesh");
        if (mesh != null) mesh.gameObject.SetActive(false);

        // SP proxy target
        PhotonView clientPV = PhotonView.Find(spProxyTeleportID);
        if (clientPV == null) return;

        followTarget = clientPV.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.DisableMovement();

        // Spawn SP client proxy
        SpawnSPProxy(followTarget);
    }

    private void SpawnSPProxy(Transform target)
    {
        GameObject proxyPrefab = Resources.Load<GameObject>("SmallPlayerProxy");
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

    public bool IsCaptured()
    {
        return isCaptured;
    }
}
