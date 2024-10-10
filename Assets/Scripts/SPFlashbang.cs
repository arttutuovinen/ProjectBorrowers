using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPFlashbang : MonoBehaviour
{
    public GameObject itemPrefab;    // The prefab of the item to spawn

    
    public void SpawnFlashbang()
    {
        Debug.Log("Flashbang used");
        // Spawn the collected item at the player's position
        Instantiate(itemPrefab, transform.position + transform.forward, Quaternion.identity);
    }
}
