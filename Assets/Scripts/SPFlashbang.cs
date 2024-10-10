using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPFlashbang : MonoBehaviour
{
    public GameObject objectToThrow;  // The prefab of the object to throw
    public Transform throwOrigin;     // The point where the object is thrown from (e.g., player's hand or camera position)
    public float throwForce = 10f;    // The force applied to the thrown object

    public Camera playerCamera;       // Reference to the player's camera

    private float timeInSeconds = 2.0f;

    void Update()
    {
        // Check if the player presses the "P1UseItem" button
        if (Input.GetButtonDown("P1UseItem"))
        {
            SpawnFlashbang();
        }
    }
    public void SpawnFlashbang()
    {
        // Create an instance of the object at the throw origin position
        GameObject thrownObject = Instantiate(objectToThrow, throwOrigin.position, Quaternion.identity);

        // Calculate the throw direction based on where the player's camera is looking
        Vector3 throwDirection = playerCamera.transform.forward;

        // Get the Rigidbody component of the thrown object
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();

        // Apply force to the Rigidbody to throw the object in the calculated direction
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        Destroy(thrownObject, timeInSeconds);

    }
}
