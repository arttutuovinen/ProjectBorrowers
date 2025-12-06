using UnityEngine;

public class SPProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;
    public float yRotationOffset = 180f; // offset to face BP

    void LateUpdate()
    {
        if (teleportTarget == null) return;

        transform.position = teleportTarget.position;

        // Apply teleport rotation + Y offset
        Vector3 euler = teleportTarget.rotation.eulerAngles;
        euler.y += yRotationOffset;
        transform.rotation = Quaternion.Euler(euler);
    }
}
