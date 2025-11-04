using System.Collections; // ✅ Needed for IEnumerator
using UnityEngine;
using Photon.Pun;

public class SPFlashbang : MonoBehaviourPun
{
    [Header("Item Settings")]
    public GameObject flashbang;     // Prefab of the Flashbang (must be in Resources folder)
    public Transform throwOrigin;    // The point where the flashbang spawns from (e.g. player's hand or camera)
    private float destroyTime = 10f; // Optional auto-destroy time

    public void SpawnFlashbang()
    {
        if (!photonView.IsMine) return; // Only local player can spawn

        if (flashbang == null)
        {
            Debug.LogWarning("SPFlashbang: No flashbang prefab assigned!");
            return;
        }

        if (throwOrigin == null)
        {
            Debug.LogWarning("SPFlashbang: No throwOrigin assigned!");
            return;
        }

        // Spawn at throwOrigin's position and rotation
        Vector3 spawnPos = throwOrigin.position;
        Quaternion spawnRot = throwOrigin.rotation;

        // Photon instantiate (must be in a Resources folder)
        GameObject spawnedFlashbang = PhotonNetwork.Instantiate(flashbang.name, spawnPos, spawnRot);

        Debug.Log($"[SPFlashbang] Spawned networked flashbang: {spawnedFlashbang.name}");

        // Optional: destroy after some time (for cleanup)
        // StartCoroutine(DestroyAfterDelay(spawnedFlashbang));
    }

    private IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(destroyTime);
        if (obj != null && obj.GetComponent<PhotonView>()?.IsMine == true)
        {
            PhotonNetwork.Destroy(obj);
        }
    }
}


