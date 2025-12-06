using UnityEngine;

public class SPProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;
    public Camera bpCamera;

    void LateUpdate()
    {
        if (teleportTarget == null) return;

        // Follow teleport position
        transform.position = teleportTarget.position;

        // Look at BP camera
        if (bpCamera != null)
        {
            Vector3 lookDir = bpCamera.transform.position - transform.position;
            lookDir.y = 0f; // optional: keep proxy upright
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
