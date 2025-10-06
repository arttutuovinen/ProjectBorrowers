using UnityEngine;
using System.Collections;

public class BPVacuumCleaner : MonoBehaviour
{
    [Header("Vacuum Settings")]
    public float vacuumRadius = 3f;
    public float pullDuration = 0.5f;
    public float pullForce = 15f;

    [Header("Area Offset")]
    public Vector3 areaOffset = new Vector3(0f, 0f, 0.5f);


    [Header("Layer Mask")]
    public LayerMask smallPlayerLayer;

    void Update()
    {
        if (Input.GetButtonDown("P2PickUp"))
        {
            TryPullTarget();
        }
    }

    private void TryPullTarget()
    {
        Vector3 center = transform.position + transform.TransformDirection(areaOffset);
        Collider[] hits = Physics.OverlapSphere(center, vacuumRadius, smallPlayerLayer);

        if (hits.Length == 0) return;

        // Pick closest small player
        Collider closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(center, hit.transform.position);
            if (dist < closestDist)
            {
                closest = hit;
                closestDist = dist;
            }
        }

        if (closest != null)
        {
            SPPullTarget pullTarget = closest.GetComponent<SPPullTarget>();
            if (pullTarget != null)
            {
                pullTarget.PullTowards(transform, pullForce, pullDuration);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying
            ? transform.position + transform.TransformDirection(areaOffset)
            : transform.position + areaOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, vacuumRadius);
    }
}
