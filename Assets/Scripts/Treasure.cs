using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public string playerTag = "SmallPlayer";
    private CompassBar compass;
    public GameObject treasure;

    void Star()
    {
        compass = FindObjectOfType<CompassBar>();
    }
    // This method is called when the player enters a trigger (Treasure is a trigger)
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has collided with the treasure
        if (other.CompareTag(playerTag))
        {
            other.gameObject.GetComponent<SmallPlayerMovement>().isTreasureCollected = true;  // Set treasure as collected
            Debug.Log("Treasure collected!");
            treasure.SetActive(false);   
            compass.CollectTreasure();
        }
    }
}
