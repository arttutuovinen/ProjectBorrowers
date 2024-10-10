using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BigPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    
    public float gravity = -9.81f; // Gravity applied to the player
    public float turnSmoothTime = 0.1f; // Smoothing for rotation

    public Transform cameraTransform; // Reference to the main camera for directional movement
    public Transform cameraFollowTarget; // Target for the camera to follow (usually the player)

    // ** Crouching variables **
    public float crouchSpeed = 2.5f; // Speed while crouching
    public float crouchTransitionSpeed, normalHeight, crouchHeight;
    public Vector3 offset;
    public Transform bigPlayer;
    bool isCrouching;
    private float currentSpeed;
    //Looking variables
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
    
    public float rayDistance = 10f;  // Maximum distance the ray should check.
    private Vector3 rayPosition;
    public Camera playerCamera;      // Reference to the player's camera.

    //Calculating rayPosition
    public float offsetX = 1f;
    public float offsetY = 1f;
    public float offsetZ = 0f;
    public TextMeshProUGUI crossHair; // crossHair text object

    // ** door interaction **
    public LayerMask doorLayerMask;
    public TextMeshProUGUI doorInteractText;
    private Door currentDoor;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen

        // Hide the interact text at the start
        if (doorInteractText != null)
        {
            doorInteractText.gameObject.SetActive(false); // Disable the text object initially
        }
        // Initially, set the current speed to normal
        currentSpeed = moveSpeed;
    }
    
    private void Update()
    {
        Move();
        ApplyGravity();
        ControlCamera();
        HandleCrouch();

        //Camera Offset manager
        rayPosition = playerCamera.transform.position + new Vector3(offsetX, offsetY, offsetZ);

        // Cast a ray from the camera's position and forward direction.
        Ray ray = new Ray(rayPosition, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue);
        // ** Door interaction detection **
        // Check if the ray hits any object within the specified distance and on the door layer
        if (Physics.Raycast(ray, out hit, rayDistance, doorLayerMask))
        {
            // Check if the object hit has a DoorInteraction component
            Door door = hit.collider.GetComponent<Door>();
            if (door != null)
            {
                // If the player presses the interact key, toggle the door
                if (Input.GetButtonDown("P2Interact"))
                {
                    door.ToggleDoor();
                }
                // Display interaction text
                if (doorInteractText != null)
                {
                    doorInteractText.gameObject.SetActive(true); // Activate the text when the door is in range
                }
                // Set the current door reference
                currentDoor = door;
            }  
        }
        else
        {
            // If the ray doesn't hit a door or is too far, deactivate the text
            if (doorInteractText != null)
            {
                doorInteractText.gameObject.SetActive(false); // Deactivate the text when no door is in range
            }

            // Clear the current door reference
            currentDoor = null;
        }

        
    }

   

    //Player Movement
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
        float horizontal = Input.GetAxisRaw("P2Horizontal"); // WASD or PS5 Left Stick X
        float vertical = Input.GetAxisRaw("P2Vertical"); // WASD or PS5 Left Stick Y
        
        // Calculate the movement direction relative to the camera's orientation
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Calculate the target angle for rotation based on camera orientation
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            // Smoothly rotate towards the target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            /// Move in the direction the player is facing
            
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }   
    }

    private void ApplyGravity()
    {
        // Apply gravity over time
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ControlCamera()
    {
        // Camera control with mouse or PS5 controller's right stick
        float mouseX, mouseY;

        if (Mathf.Abs(Input.GetAxis("P2RightStickHorizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("P2RightStickVertical")) > 0.1f)
        {
            // PS5 right stick
            mouseX = Input.GetAxis("P2RightStickHorizontal") * controllerSensitivity;
            mouseY = Input.GetAxis("P2RightStickVertical") * controllerSensitivity;
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
    private void HandleCrouch()
    {
        // Toggle crouch state when P2Crouch is pressed
        if (Input.GetButtonDown("P2Crouch"))
        {
            isCrouching = !isCrouching;
            
        }
        if (isCrouching)
        {
            controller.height = controller.height - crouchSpeed * Time.deltaTime;
            if (controller.height <= crouchHeight)
            {
                controller.height = crouchHeight;
            }
            // Set the player's speed to crouch speed
            moveSpeed = crouchSpeed;
        }
        else
        {
            controller.height = controller.height + crouchSpeed * Time.deltaTime;
            if (controller.height < normalHeight)
            {
                bigPlayer.gameObject.SetActive(false);
                bigPlayer.position = bigPlayer.position + offset * Time.deltaTime;
                bigPlayer.gameObject.SetActive(true);
            }
            if (controller.height >= normalHeight)
            {
                controller.height = normalHeight;
            }
            // Set the player's speed back to normal when not crouching
            moveSpeed = 10f;
        }
        
    }


}
