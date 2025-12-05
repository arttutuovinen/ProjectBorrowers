using UnityEngine;
using Photon.Pun;

public class BPCameraFollower : MonoBehaviourPun
{
    [Header("Manual Offset (local to camera)")]
    [SerializeField] private float offsetX = 0f;     // Left/Right
    [SerializeField] private float offsetY = 0f;     // Height
    [SerializeField] private float offsetZ = 0f;     // Forward/Backward

    public Transform smallPlayerTeleportPos;

    void LateUpdate()
    {
        if (smallPlayerTeleportPos == null) return;

        // Calculate world offset from camera
        Vector3 worldOffset =
            transform.right * offsetX +
            transform.up * offsetY +
            transform.forward * offsetZ;

        // Apply to teleport position
        smallPlayerTeleportPos.position = transform.position + worldOffset;
        smallPlayerTeleportPos.rotation = transform.rotation;
    }
}
