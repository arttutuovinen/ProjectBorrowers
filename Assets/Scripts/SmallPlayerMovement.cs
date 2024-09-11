using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Speed at which the player moves
    public float rotateSpeed = 700f;  // Speed at which the player rotates

    void Update()
    {
        // Get input from the player (WASD or Arrow keys)
        float moveX = Input.GetAxis("Horizontal");  // Left/Right (A/D or Left/Right arrow)
        float moveZ = Input.GetAxis("Vertical");    // Forward/Backward (W/S or Up/Down arrow)

        // Move the player forward and backward
        Vector3 move = transform.forward * moveZ * moveSpeed * Time.deltaTime;

        // Move the player forward and backward
        Vector3 move2 = transform.right * moveX * moveSpeed * Time.deltaTime;

        // Apply the movement to the player's position
        transform.Translate(move + move2, Space.World);

        

        // Rotate the player left and right
        // float rotate = moveX * rotateSpeed * Time.deltaTime;
        // transform.Rotate(0, rotate, 0);
    }
}
