using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    private GameObject[] spawnPoints;
    private Transform chosenSpawnPoint;

    private void Awake()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(AssignSpawnAfterFrame(clientId));
    }

    private IEnumerator AssignSpawnAfterFrame(ulong clientId)
    {
        yield return null; // Wait one frame so the player actually exists

        if (chosenSpawnPoint == null)
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points found!");
                yield break;
            }

            int randomIndex = Random.Range(0, spawnPoints.Length);
            chosenSpawnPoint = spawnPoints[randomIndex].transform;
            Debug.Log($"[Spawner] Chosen spawn: {chosenSpawnPoint.name}");
        }

        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObj == null)
        {
            Debug.LogWarning("Player object not found after connect.");
            yield break;
        }

        // Move safely
        var cc = playerObj.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        playerObj.transform.SetPositionAndRotation(chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        var netTransform = playerObj.GetComponent<Unity.Netcode.Components.NetworkTransform>();
        if (netTransform)
        {
            netTransform.Teleport(chosenSpawnPoint.position, chosenSpawnPoint.rotation, Vector3.one);
        }

        if (cc)
        {
            yield return new WaitForFixedUpdate();
            cc.enabled = true;
        }
    }

    private void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}

