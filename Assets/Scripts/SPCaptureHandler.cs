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
    public void RPC_CapturePlayer(int proxyTeleportID, int clientTeleportID)
    {
        if (!photonView.IsMine) return;

        // Hide SmallPlayerMesh locally
        Transform mesh = transform.Find("SmallPlayerMesh");
        if (mesh != null)
            mesh.gameObject.SetActive(false);

        // Find SPClientTeleportLocation to follow
        PhotonView clientPV = PhotonView.Find(clientTeleportID);
        if (clientPV == null) return;

        followTarget = clientPV.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.DisableMovement();

        BPCatchController bpController = FindObjectOfType<BPCatchController>();
        if (bpController != null)
            bpController.PlayCaughtReaction();
    }

    private void Update()
    {
        if (!isCaptured || followTarget == null || !photonView.IsMine) return;

        controller.enabled = false;
        transform.position = Vector3.Lerp(transform.position, followTarget.position, Time.deltaTime * 20f);
        controller.enabled = true;
    }

    public bool IsCaptured()
    {
        return isCaptured;
    }
}
