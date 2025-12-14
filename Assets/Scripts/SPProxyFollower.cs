using UnityEngine;

public class SPProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;
    public Camera bpCamera;

    void LateUpdate()
    {
        if (teleportTarget == null) return;

        transform.position = teleportTarget.position;

        if (bpCamera != null)
        {
            Vector3 lookDir = bpCamera.transform.position - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
}
