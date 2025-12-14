using UnityEngine;

public class SPClientProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;    // SPClientTeleportLocation
    public Transform bpTransform;       // BP prefab root to look at
    public float yRotationOffset = 0f;  // Adjust if mesh faces wrong way

    void LateUpdate()
    {
        if (teleportTarget == null) return;

        // Follow SP client teleport position
        transform.position = teleportTarget.position;

        // Face BP prefab
        if (bpTransform != null)
        {
            Vector3 lookDir = bpTransform.position - transform.position;
            lookDir.y = 0f; // keep upright

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
                rot *= Quaternion.Euler(0f, yRotationOffset, 0f);
                transform.rotation = rot;
            }
        }
    }
}
