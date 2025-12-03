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
        if (!photonView.IsMine)
            return; // Only local player controls their teleport + movement disable

        isCaptured = true;
        followTarget = teleportPos;

        // Teleport immediately
        controller.enabled = false;
        transform.position = teleportPos.position;
        controller.enabled = true;

        // Disable movement
        if (movementScript != null)
            movementScript.enabled = false;
    }

    void Update()
    {
        if (!isCaptured) return;

        // Disable CharacterController temporarily
        controller.enabled = false;

        // Follow the teleport target on all axes
        Vector3 targetPos = followTarget.position;
        transform.position = targetPos;

        // Re-enable controller
        controller.enabled = true;
    }
}
