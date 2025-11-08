using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BigPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Speed of movement

    public float gravity = -9.81f; // Gravity applied to the player
    public float turnSmoothTime = 0.1f; // Smoothing for rotation

    public Transform cameraTransform; // Reference to the main camera for directional movement
    public Transform cameraFollowTarget; // Target for the camera to follow (usually the player)

    // ** Crouching variables **
    public float crouchSpeed = 1.0f;     // Speed when crouching
    [HideInInspector] public float currentSpeed;
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public Vector3 standingCenter = Vector3.zero;
    public Vector3 crouchingCenter = new Vector3(0, -0.5f, 0);
    private bool isCrouching = false;

    public float crouchCameraYOffset = -0.5f;
    public float cameraTransitionTime = 0.5f;
    private float originalCameraY;

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
    public LayerMask lightSwitchLayerMask;          // Layer mask for switches
    private LightSwitchInteraction currentLightSwitch;          // Track the switch you're looking at

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

        controller.height = standingHeight;
        controller.center = standingCenter;
        originalCameraY = cameraFollowTarget.localPosition.y;
    }

    private void Update()
    {
        Move();
        ApplyGravity();
        ControlCamera();
        HandleCrouchInput();


        //Camera Offset manager
        rayPosition = playerCamera.transform.position + new Vector3(offsetX, offsetY, offsetZ);

        // Reset interaction reference each frame
        currentDoor = null;
        currentLightSwitch = null;

        // Cast a ray from the camera's position and forward direction.
        Ray ray = new Ray(rayPosition, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue);

        // Check if the ray hits any object within range
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // --- Door check ---
            Door door = hit.collider.GetComponent<Door>();
            if (door != null && ((1 << hit.collider.gameObject.layer) & doorLayerMask) != 0)
            {
                if (Input.GetButtonDown("P2Interact"))
                {
                    door.ToggleDoor();
                }

                currentDoor = door;
                if (doorInteractText != null)
                    doorInteractText.gameObject.SetActive(true);
                return; // Stop here so only one interaction is shown at a time
            }

            // --- Light switch check ---
            LightSwitchInteraction lightSwitch = hit.collider.GetComponent<LightSwitchInteraction>();
            if (lightSwitch != null && ((1 << hit.collider.gameObject.layer) & lightSwitchLayerMask) != 0)
            {
                if (Input.GetButtonDown("P2Interact"))
                {
                    lightSwitch.Interact();
                }

                currentLightSwitch = lightSwitch;
                if (doorInteractText != null) // reuse the same UI text
                    doorInteractText.gameObject.SetActive(true);
                return;
            }
        }

        // If nothing is hit, hide the text
        if (doorInteractText != null)
            doorInteractText.gameObject.SetActive(false);


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
        float horizontal = Input.GetAxisRaw("Horizontal"); // WASD or PS5 Left Stick X
        float vertical = Input.GetAxisRaw("Vertical"); // WASD or PS5 Left Stick Y

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
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
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
    private void HandleCrouchInput()
    {
        if (Input.GetButtonDown("P2Crouch"))
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height = crouchingHeight;
                controller.center = crouchingCenter;
                currentSpeed = crouchSpeed;

                if (cameraFollowTarget != null)
                    StartCoroutine(AdjustCameraTarget(originalCameraY + crouchCameraYOffset));
            }
            else
            {
                controller.height = standingHeight;
                controller.center = standingCenter;
                currentSpeed = moveSpeed;

                if (cameraFollowTarget != null)
                    StartCoroutine(AdjustCameraTarget(originalCameraY));
            }
        }
    }
    
    private IEnumerator AdjustCameraTarget(float targetY)
    {
        Vector3 startPos = cameraFollowTarget.localPosition;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);
        float elapsed = 0f;

        while (elapsed < cameraTransitionTime)
        {
            float t = elapsed / cameraTransitionTime;
            cameraFollowTarget.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraFollowTarget.localPosition = endPos; // Snap to final position
    }
}
