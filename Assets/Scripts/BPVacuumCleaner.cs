using UnityEngine;
using Photon.Pun;

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

    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    // Called by your input or animation event
    public void TryPullTarget()
    {
        // Only the local owner performs detection
        if (!pv.IsMine) return;

        Vector3 center = transform.position + transform.TransformDirection(areaOffset);

        Collider[] hits = Physics.OverlapSphere(center, vacuumRadius, smallPlayerLayer);
        if (hits.Length == 0) return;

        // Find closest target
        Collider closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            float dist = Vector3.Distance(center, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit;
            }
        }

        if (closest == null) return;

        PhotonView targetPv = closest.GetComponent<PhotonView>();
        if (targetPv == null) return;

        // Tell all players to pull this exact target
        pv.RPC("RPC_PullTarget", RpcTarget.All, targetPv.ViewID);
    }

    [PunRPC]
    void RPC_PullTarget(int targetViewID)
    {
        PhotonView targetPv = PhotonView.Find(targetViewID);
        if (targetPv == null) return;

        SPPullTarget pullTarget = targetPv.GetComponent<SPPullTarget>();
        if (pullTarget != null)
        {
            pullTarget.PullTowards(transform, pullForce, pullDuration);
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
