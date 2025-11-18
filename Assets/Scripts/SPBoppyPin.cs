using UnityEngine;
using Photon.Pun;

public class SPBoppyPin : MonoBehaviourPun
{
    [Header("Item Settings")]
    public GameObject itemPrefab; // Must be inside a Resources folder

    public void SpawnBoppyPin()
    {
        if (!photonView.IsMine) return; // Only local player can spawn

        if (itemPrefab == null)
        {
            Debug.LogWarning("SPBoppyPin: No itemPrefab assigned!");
            return;
        }

        // Spawn slightly in front of the SmallPlayer
        Vector3 spawnPos = transform.position + transform.forward * 1.0f;

        // Spawn over the network
        GameObject spawnedItem = PhotonNetwork.Instantiate(itemPrefab.name, spawnPos, Quaternion.identity);
        var rb = spawnedItem.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // --- FLOOR SNAP (Raycast Down) ---
        if (Physics.Raycast(spawnedItem.transform.position, Vector3.down, out RaycastHit hit, 5f))
        {
            spawnedItem.transform.position = hit.point;
        }

        Debug.Log($"[SPBoppyPin] Spawned and dropped item: {spawnedItem.name}");
    }
}

