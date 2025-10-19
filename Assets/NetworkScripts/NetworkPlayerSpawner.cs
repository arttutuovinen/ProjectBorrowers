using UnityEngine;
using Unity.Netcode;


public class NetworkPlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;   // Your player prefab
    public Transform[] spawnPoints;   // Possible spawn points

    private Vector3 sharedSpawnPos;   // Same spawn for all players

    private void Start()
    {
        // Spawn host player if we are the host
        if (NetworkManager.Singleton.IsHost)
        {
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
        }

        // Subscribe to client connections
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Pick a random spawn point (only once)
        if (sharedSpawnPos == Vector3.zero && spawnPoints.Length > 0)
        {
            sharedSpawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        GameObject playerInstance = Instantiate(playerPrefab, sharedSpawnPos, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}

