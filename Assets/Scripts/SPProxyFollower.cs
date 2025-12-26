using UnityEngine;

public class SPProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;
    public Camera bpCamera;

    void LateUpdate()
    {
        if (!teleportTarget) return;

        transform.position = teleportTarget.position;

        if (bpCamera)
        {
            Vector3 dir = bpCamera.transform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
