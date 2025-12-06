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
        // Only SP owner moves their own player
        if (!photonView.IsMine) return;

        PhotonView pv = PhotonView.Find(teleportTargetID);
        if (pv == null)
        {
            Debug.LogError("Teleport target not found!");
            return;
        }

        followTarget = pv.transform;
        isCaptured = true;

        if (movementScript != null)
            movementScript.enabled = false;
    }

    private void Update()
    {
        if (!isCaptured || followTarget == null || !photonView.IsMine) return;

        controller.enabled = false;
        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;
        controller.enabled = true;
    }
}
