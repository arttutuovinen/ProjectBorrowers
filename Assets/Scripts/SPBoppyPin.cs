using UnityEngine;
using Photon.Pun;

public class SPBoppyPin : MonoBehaviourPun
{
    [Header("Item Settings")]
    public GameObject itemPrefab; // The prefab of the item to spawn (must be in Resources folder)

    public void SpawnBoppyPin()
    {
        if (!photonView.IsMine) return; // Only local player can spawn

        if (itemPrefab == null)
        {
            Debug.LogWarning("SPBoppyPin: No itemPrefab assigned!");
            return;
        }

        // The prefab must exist under a "Resources" folder for PhotonNetwork.Instantiate to work
        Vector3 spawnPos = transform.position + transform.forward * 1.0f;
        Quaternion spawnRot = Quaternion.identity;

        // Spawn network-wide
        GameObject spawnedItem = PhotonNetwork.Instantiate(itemPrefab.name, spawnPos, spawnRot);

        Debug.Log($"[SPBoppyPin] Spawned networked item: {spawnedItem.name}");
    }
}
