using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPThrowItem : MonoBehaviour
{
    public GameObject throwItem;
    public Transform throwOrigin;     // The point where the object is thrown from (e.g., player's hand or camera position)
    public float throwForce = 10f;    // The force applied to the thrown object
    private float destroyTime = 2f;
    public Camera playerCamera;       // Reference to the player's camera


    public void SpawnThrowItem()
    {
        // Spawn the collected item at the player's position
        GameObject thrownObject = Instantiate(throwItem, transform.position + transform.forward, Quaternion.identity);
        // Calculate the throw direction based on where the player's camera is looking
        Vector3 throwDirection = playerCamera.transform.forward;
        // Get the Rigidbody component of the thrown object
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
        // Apply force to the Rigidbody to throw the object in the calculated direction
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        Destroy(thrownObject, destroyTime);
    }
}
