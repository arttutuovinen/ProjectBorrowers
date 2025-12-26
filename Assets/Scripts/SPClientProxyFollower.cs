using UnityEngine;

public class SPClientProxyFollower : MonoBehaviour
{
    public Transform teleportTarget;
    public Transform bpTransform;

    void LateUpdate()
    {
        if (!teleportTarget) return;

        transform.position = teleportTarget.position;

        if (bpTransform)
        {
            Vector3 dir = bpTransform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }   
}
