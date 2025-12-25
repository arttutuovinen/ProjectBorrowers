using UnityEngine;
using Photon.Pun;

public class SPBoppyPin : MonoBehaviourPun
{
    [Header("Item Settings")]
    public GameObject itemPrefab;
    public Transform throwOrigin;
    public LayerMask groundMask; // Ground only (NOT SmallPlayer)

    public void SpawnBoppyPin()
    {
        if (!photonView.IsMine) return;
        if (itemPrefab == null || throwOrigin == null) return;

        Vector3 rayStart = throwOrigin.position + Vector3.up * 3f;
        Vector3 finalSpawnPos = throwOrigin.position;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            20f,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            finalSpawnPos = hit.point + Vector3.up * 0.05f;
        }
        else
        {
            Debug.LogError("GROUND NOT HIT — CHECK LAYERS");
        }

        // ⬅️ INSTANTIATE AT FINAL POSITION
        PhotonNetwork.Instantiate(
            itemPrefab.name,
            finalSpawnPos,
            Quaternion.identity
        );
    }

}
