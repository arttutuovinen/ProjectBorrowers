using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlayerMovement : MonoBehaviour
{

    public float moveSpeed = 5f; // Speed of movement
    public float climbSpeed = 3f; // Speed for climbing ladders
    public float gravity = -9.81f; // Gravity applied to the player
    public float jumpHeight = 1.5f; // How high the player can jump
    public float turnSmoothTime = 0.1f; // Smoothing for rotation

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

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen
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

    private void Move()
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
        // Camera control with mouse or PS5 controller's right stick
        float mouseX, mouseY;

        if (Mathf.Abs(Input.GetAxis("RightStickHorizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("RightStickVertical")) > 0.1f)
        {
            // PS5 right stick
            mouseX = Input.GetAxis("RightStickHorizontal") * controllerSensitivity;
            mouseY = Input.GetAxis("RightStickVertical") * controllerSensitivity;
        }
        else
        {
            // Mouse input
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }

        // Update yaw (horizontal rotation) and clamp pitch (vertical rotation)
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Rotate the camera around the player
        cameraTransform.position = cameraFollowTarget.position - (Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward * distanceFromPlayer);

        // Look at the player
        cameraTransform.LookAt(cameraFollowTarget.position);
    }

    // Detect when the player is near a ladder (use triggers or raycast)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
            ladder = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == ladder)
        {
            nearLadder = false;
            ladder = null;
            isClimbing = false; // Stop climbing when leaving the ladder
        }
    }
}
