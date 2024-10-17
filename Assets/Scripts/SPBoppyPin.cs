using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBoppyPin : MonoBehaviour
{
    public GameObject itemPrefab;    // The prefab of the item to spawn
   
    public void SpawnBoppyPin()
    {
        // Spawn the collected item at the player's position
        Instantiate(itemPrefab, transform.position + transform.forward, Quaternion.identity);
    }
}
