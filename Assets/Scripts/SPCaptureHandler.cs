using UnityEngine;
using Photon.Pun;

public class SPCaptureHandler : MonoBehaviourPun
{
    private CharacterController controller;
    private SPMovementNET movementScript;

    private Transform followTarget; // where to stay after being captured
    private bool isCaptured = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<SPMovementNET>(); // your movement script name
    }

    [PunRPC]
    public void RPC_CapturePlayer(int teleportTargetID)
    {
        if (!photonView.IsMine) return;

        Transform target = PhotonView.Find(teleportTargetID).transform;
        Capture(target);
    }

    public void Capture(Transform teleportPos)
    {
        if (!photonView.IsMine) return;

        isCaptured = true;
        followTarget = teleportPos;

        // Disable controller before changing parent/position
        if (controller != null) controller.enabled = false;

        // Parent the small player to the teleport target so it follows position & rotation exactly
        transform.SetParent(followTarget, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable movement script
        if (movementScript != null) movementScript.enabled = false;
    }

    void Update()
    {
        if (!isCaptured) return;
    }
}
