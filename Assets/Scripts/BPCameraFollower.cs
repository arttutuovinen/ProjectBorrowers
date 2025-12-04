using UnityEngine;
using Photon.Pun;

public class BPCameraFollower : MonoBehaviourPun
{
    [SerializeField] private Transform smallPlayerTeleportPos;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Start()
    {
        if (smallPlayerTeleportPos != null)
        {
            // Store local position and rotation relative to camera
            initialLocalPos = smallPlayerTeleportPos.localPosition;
            initialLocalRot = smallPlayerTeleportPos.localRotation;
        }
    }

    void LateUpdate()
    {
        if (!photonView.IsMine || smallPlayerTeleportPos == null) return;

        // Follow camera rotation only, preserve prefab world position
        smallPlayerTeleportPos.rotation = transform.rotation * initialLocalRot;

        // Keep X and Z following camera direction, keep Y the original prefab Y
        Vector3 forwardOffset = transform.forward * initialLocalPos.z + transform.right * initialLocalPos.x;
        smallPlayerTeleportPos.position = new Vector3(
            transform.position.x + forwardOffset.x,
            initialLocalPos.y,   // preserve prefab Y
            transform.position.z + forwardOffset.z
        );
    }
}
