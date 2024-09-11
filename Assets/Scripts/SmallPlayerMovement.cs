using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Speed at which the player moves
    public float rotateSpeed = 700f;  // Speed at which the player rotates

    public float jumpForce = 5f;   // The force applied to make the player jump
    public bool isGrounded = true; // To check if the player is grounded

    private Rigidbody rb;          // Reference to the Rigidbody component

    void Start()
    {
        // Get the Rigidbody component attached to the player
        rb = GetComponent<Rigidbody>();
        
    }

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

        
        
        // Check if the player presses the Jump button (Space bar by default) and is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }


    }

    // Function to make the player jump
    void Jump()
    {
        // Apply a vertical force to the Rigidbody to make the player jump
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;  // Once the player jumps, they are no longer grounded
    }

    // Detect when the player lands back on the ground
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the player collided with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;  // Player is grounded again
        }
    }
}
