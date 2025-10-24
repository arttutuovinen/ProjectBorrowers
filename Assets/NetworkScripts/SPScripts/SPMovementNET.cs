using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;        // Max running speed
    public float acceleration = 40f;    // Higher = snappier movement
    public float jumpForce = 11f;       // Jump ~3x player height

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayers;      // ✅ Added — choose which layers count as ground

    [Header("Camera Settings")]
    public Transform cameraFollowTarget;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;

    [Header("Gravity Settings")]
    public float fallMultiplier = 2.5f;     // Faster falling
    public float lowJumpMultiplier = 2f;    // Short hops

    private Rigidbody rb;
    private bool isGrounded;

    private float cameraXRotation = 20f;
    private float cameraYRotation = 0f;

    private Vector3 moveInput;
    private Camera playerCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.isKinematic = false;

        // Make sure damping (drag) values are reasonable
        rb.linearDamping = 6f;   // replaces "drag" — reduces sliding
        rb.angularDamping = 0.05f;

        playerCamera = GameObject.Find("SmallPlayerCamera").GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleInput();
        HandleMouseLook();
        HandleCamera();
        CheckGrounded();
        HandleJump();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyBetterGravity();
    }

    void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

        // Camera-relative movement
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveInput = (camForward * vertical + camRight * horizontal).normalized;
    }

    void ApplyMovement()
    {
        // Get the desired velocity on the XZ plane
        Vector3 targetVelocity = moveInput * moveSpeed;

        // Current horizontal velocity
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Acceleration towards target velocity
        Vector3 velocityChange = (targetVelocity - currentVelocity) * acceleration;

        // If near a wall, project movement along wall to prevent sticking
        if (isGrounded)
        {
            // When grounded, just apply as usual
            rb.AddForce(velocityChange, ForceMode.Acceleration);
        }
        else
        {
            // When airborne, check if hitting a wall
            if (Physics.Raycast(transform.position, moveInput, out RaycastHit hit, 0.6f))
            {
                if (!hit.collider.CompareTag("SmallPlayer"))
                {
                    // Slide along the wall instead of pushing into it
                    Vector3 wallNormal = hit.normal;
                    Vector3 slideDir = Vector3.ProjectOnPlane(moveInput, wallNormal).normalized;
                    Vector3 slideVelocity = slideDir * moveSpeed;

                    Vector3 airborneVelocityChange = (slideVelocity - currentVelocity) * acceleration * 0.5f;
                    rb.AddForce(airborneVelocityChange, ForceMode.Acceleration);
                    return;
                }
            }

            // Normal mid-air movement (no wall hit)
            rb.AddForce(velocityChange * 0.5f, ForceMode.Acceleration);
        }

        // Rotate toward move direction if moving
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
        // Stop tiny residual movement when no input
        if (moveInput.sqrMagnitude < 0.01f && isGrounded)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude < 0.2f)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }


    void HandleJump()
    {
        if (isGrounded && (Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling — apply stronger gravity
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Early jump release — smaller hop
            rb.AddForce(Physics.gravity * (lowJumpMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    void CheckGrounded()
    {
        // ✅ Only detects colliders in the specified ground layers
        isGrounded = Physics.CheckSphere(groundCheck.position,groundDistance,groundLayers);

        // Debug visualization
        Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, isGrounded ? Color.green : Color.red, 0.1f);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        cameraYRotation += mouseX;
        cameraXRotation -= mouseY;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -35f, 60f);
    }

    void HandleCamera()
    {
        if (playerCamera == null || cameraFollowTarget == null) return;

        Quaternion rotation = Quaternion.Euler(cameraXRotation, cameraYRotation, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -cameraDistance);

        playerCamera.transform.position = cameraFollowTarget.position + offset + Vector3.up * cameraHeight;
        playerCamera.transform.LookAt(cameraFollowTarget.position + Vector3.up * cameraHeight);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance); // ✅ visualize sphere radius
        }
    }
}


