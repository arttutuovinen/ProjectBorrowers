using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SmallPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public float climbSpeed = 3f; // Speed for climbing ladders
    public float gravity = -9.81f; // Gravity applied to the player
    public float jumpHeight = 1.5f; // How high the player can jump

    public float turnSmoothTime = 0.1f; // Smoothing for rotation
    private bool canMove = true; // A flag to control movement

    public Transform cameraTransform; // Reference to the main camera for directional movement
    public Transform cameraFollowTarget; // Target for the camera to follow (usually the player)

    public float mouseSensitivity = 100f; // Mouse sensitivity for camera movement
    public float controllerSensitivity = 2f; // Controller sensitivity for camera movement
    public float distanceFromPlayer = 5f; // Distance of the camera from the player
    public float minVerticalAngle = -30f; // Minimum vertical angle for camera
    public float maxVerticalAngle = 60f; // Maximum vertical angle for camera

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float turnSmoothVelocity;

    private float yaw; // Horizontal rotation
    private float pitch; // Vertical rotation

    // Ladder climbing variables
    private bool isClimbing = false; // Is the player currently climbing a ladder?
    private bool nearLadder = false; // Is the player near a ladder?
    private Collider ladder; // Reference to the ladder the player is interacting with

    //Treasure Collection
    public bool isTreasureCollected = false;

    //Ladder text
    public TextMeshProUGUI ladderInteractText;
    
    public TextMeshProUGUI itemInteractText;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen
 
        // Hide the interact text at the start
        if (ladderInteractText != null)
        {
            ladderInteractText.gameObject.SetActive(false); // Disable the text object initially
        }

    }

    private void Update()
    {
        
        if (isClimbing)
        {
            ClimbLadder();
        }
        else
        {
            Move();
            ApplyGravity();
        }
        ControlCamera();
        
    }
    // Method that allows enabling movement from other scripts
    public void EnableMovement()
    {
        canMove = true;
        Debug.Log("Movement enabled.");
    }

    // Method that allows disabling movement from other scripts
    public void DisableMovement()
    {
        canMove = false;
        Debug.Log("Movement disabled.");
    }

    public void Move()
    {
        if (canMove)
        {
            // Check if the player is on the ground
            isGrounded = controller.isGrounded;

            // Reset velocity if on the ground
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Get input for movement (WASD/arrow keys or PS5 left stick)
            float horizontal = Input.GetAxisRaw("P1Horizontal"); // WASD or PS5 Left Stick X
            float vertical = Input.GetAxisRaw("P1Vertical"); // WASD or PS5 Left Stick Y
                                                             //Debug.Log(horizontal +" "+vertical);

            // Calculate the movement direction relative to the camera's orientation
            Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                // Calculate the target angle for rotation based on camera orientation
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                // Smoothly rotate towards the target angle
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // Move in the direction the player is facing
                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
            }

            // Jumping mechanic
            // PS5 Cross button (Button 0) or Space key for jump
            if ((Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);   
            }

            // Ladder interaction: press E to start climbing if near a ladder
            if (nearLadder && Input.GetKeyDown(KeyCode.E) || nearLadder && Input.GetButtonDown("P1Interact"))
            {
                isClimbing = true;
                velocity.y = 0f; // Reset vertical velocity
            }
        }
    }

    private void ApplyGravity()
    {
        // Apply gravity over time
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
    }

    private void ClimbLadder()
    {
        // Disable gravity while climbing
        velocity.y = 0f;

        // Get vertical input for climbing (W and S keys or PS5 D-Pad Up/Down)
        float vertical = Input.GetAxisRaw("P1Vertical");

        // Move the player up or down the ladder
        Vector3 climbDirection = new Vector3(0, vertical, 0).normalized;
        controller.Move(climbDirection * climbSpeed * Time.deltaTime);

        // Exit climbing mode when the player presses E again
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("P1Interact"))
        {
            isClimbing = false;
        }
    }

    private void ControlCamera()
    {
        float mouseX, mouseY;

        // Input handling (PS5 controller or mouse)
        if (Mathf.Abs(Input.GetAxis("P1RightStickHorizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("P1RightStickVertical")) > 0.1f)
        {
            mouseX = Input.GetAxis("P1RightStickHorizontal") * controllerSensitivity;
            mouseY = Input.GetAxis("P1RightStickVertical") * controllerSensitivity;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }

        // Update rotation
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Desired direction from player to camera
        Vector3 desiredDirection = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back;

        // Desired position of the camera (ideal distance)
        Vector3 desiredCameraPosition = cameraFollowTarget.position + desiredDirection * distanceFromPlayer;

        // Raycast to check for obstacles between target and camera
        RaycastHit hit;
        Vector3 rayOrigin = cameraFollowTarget.position;
        Vector3 rayDirection = desiredCameraPosition - rayOrigin;
        float maxDistance = distanceFromPlayer;

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, out hit, maxDistance))
        {
            // Obstacle hit: adjust camera position to hit point minus small offset
            float adjustedDistance = hit.distance - 0.3f;
            desiredCameraPosition = rayOrigin + rayDirection.normalized * Mathf.Max(adjustedDistance, 0.5f);
        }

        // Set camera position and look at the player
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredCameraPosition, Time.deltaTime * 20f);;
        cameraTransform.LookAt(cameraFollowTarget.position);
    }

    // Detect when the player is near a ladder (use triggers or raycast)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
            ladder = other;

            // Enable the interact text when the player enters the trigger
            if (ladderInteractText != null)
            {
                ladderInteractText.gameObject.SetActive(true); // Activate text when the player enters the trigger
            }
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == ladder)
        {
            nearLadder = false;
            ladder = null;
            isClimbing = false; // Stop climbing when leaving the ladder

            if (ladderInteractText != null)
            {
                ladderInteractText.gameObject.SetActive(false); // Deactivate text when the player exits the trigger
            }
        }
    }
    

}
