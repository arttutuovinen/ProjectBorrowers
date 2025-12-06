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
    public void RPC_CapturePlayer(int teleportTargetID)
    {
        if (!photonView.IsMine) return;

        PhotonView pv = PhotonView.Find(teleportTargetID);
        if (pv == null) return;

        followTarget = pv.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.DisableMovement();
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
