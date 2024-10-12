using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPThrowItem : MonoBehaviour
{
    public GameObject throwItemWeapon;
    public Transform throwOrigin;     // The point where the object is thrown from (e.g., player's hand or camera position)
    public float throwForce = 20f;    // The force applied to the thrown object
    private float destroyTime = 10f;
    public Camera playerCamera;       // Reference to the player's camera


    public void SpawnThrowItem()
    {
        // Spawn the collected item at the player's position
        GameObject thrownObject = Instantiate(throwItemWeapon, throwOrigin.transform.position, Quaternion.identity);
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
        Vector3 throwDirection = playerCamera.transform.forward;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        Destroy(thrownObject, destroyTime);
    }
}
