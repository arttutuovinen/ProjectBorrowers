using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPMouseTrap : MonoBehaviour
{
    public GameObject mouseTrap;
    public Transform throwOrigin;

    public void SpawnMouseTrap()
    {
        // Spawn the collected item at the player's position
        GameObject MouseTrapObject = Instantiate(mouseTrap, throwOrigin.transform.position, Quaternion.identity);

        // Set layer visibility
        SetMouseTrapVisibility(MouseTrapObject);
    }

    private void SetMouseTrapVisibility(GameObject mouseTrapObject)
    {
        // Create LayerMasks
        int smallPlayerLayer = LayerMask.NameToLayer("SmallPlayer");
        int defaultLayer = LayerMask.NameToLayer("BigPlayer");

        // Get all cameras in the scene
        Camera[] cameras = Camera.allCameras;

        foreach (Camera cam in cameras)
        {
            // Check if the camera belongs to Small Player
            if (cam.gameObject.layer == smallPlayerLayer)
            {
                // Exclude the mouseTrap's layer from the Small Player's culling mask
                cam.cullingMask &= ~(1 << mouseTrapObject.layer);
            }
            // Check if the camera belongs to Big Player
            else if (cam.gameObject.layer == defaultLayer)
            {
                // Ensure the mouseTrap's layer is included in the Big Player's culling mask
                cam.cullingMask |= (1 << mouseTrapObject.layer);
            }
        }
    }
}
