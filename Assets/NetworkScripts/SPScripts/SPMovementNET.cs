using UnityEngine;
using TMPro;

public class SPMovementNET : MonoBehaviour
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

    [Header("Jump Settings")]
    public float coyoteTime = 0.2f;  // Grace period after leaving ground
    private float coyoteCounter;

    [Header("Ladder Settings")]
    public float climbSpeed = 8f;
    private bool nearLadder;
    private bool isClimbing;
    private Collider currentLadder;
    private TextMeshProUGUI ladderInteractText;

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

        
        rb.linearDamping = 6f;   // replaces "drag" — reduces sliding
        rb.angularDamping = 0.05f;

        playerCamera = GameObject.Find("SmallPlayerCamera").GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        ladderInteractText = GameObject.Find("SPinteractLadder")?.GetComponent<TextMeshProUGUI>();
        ladderInteractText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleMouseLook();
        HandleCamera();
        if (nearLadder && (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("P1Interact")))
        {
            ToggleClimb();
        }
        if (isClimbing)
        {
            ClimbLadder();
            return; // ✅ stop normal movement while climbing
        }
        HandleInput();
        CheckGrounded();
        HandleJump();
    }

    void FixedUpdate()
    {
        if (isClimbing)
            return; // skip normal movement + gravity while climbing
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
        // 1️⃣ Calculate target horizontal velocity based on input
        Vector3 targetVelocity = moveInput * moveSpeed;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 velocityChange = (targetVelocity - currentVelocity) * acceleration;

        // 2️⃣ Apply movement forces
        if (isGrounded)
        {
            // Normal grounded movement
            rb.AddForce(velocityChange, ForceMode.Acceleration);
        }
        else
        {
            // Airborne movement with wall-slide prevention
            if (Physics.Raycast(transform.position, moveInput, out RaycastHit hit, 0.6f))
            {
                if (!hit.collider.CompareTag("SmallPlayer"))
                {
                    // Project movement along wall to slide instead of sticking
                    Vector3 wallNormal = hit.normal;
                    Vector3 slideDir = Vector3.ProjectOnPlane(moveInput, wallNormal).normalized;
                    Vector3 slideVelocity = slideDir * moveSpeed;
                    Vector3 airborneVelocityChange = (slideVelocity - currentVelocity) * acceleration * 0.5f;
                    rb.AddForce(airborneVelocityChange, ForceMode.Acceleration);
                    return; // skip further movement force
                }
            }

            // Normal mid-air movement (no wall hit)
            rb.AddForce(velocityChange * 0.5f, ForceMode.Acceleration);
        }

        // 3️⃣ Rotate player toward movement direction if moving
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }

        
    }

    void HandleJump()
    {
        // Allow jump if within coyote time
        if (coyoteCounter > 0f && (Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")))
        {
            // Reset vertical velocity before jumping
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Apply jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Consume coyote time
            coyoteCounter = 0f;
        }
    }

    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling — apply stronger gravity
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
        
    }

    void CheckGrounded()
    {
        // Raycast down for more reliable detection on small surfaces
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance + 0.05f, groundLayers);

        // Update coyote timer
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Debug visualization
        Debug.DrawRay(groundCheck.position, Vector3.down * (groundDistance + 0.05f), isGrounded ? Color.green : Color.red);
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

        // Desired camera position based on rotation
        Quaternion rotation = Quaternion.Euler(cameraXRotation, cameraYRotation, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, 0, -cameraDistance);
        Vector3 desiredPosition = cameraFollowTarget.position + desiredOffset + Vector3.up * cameraHeight;

        // Camera collision logic
        Vector3 targetCenter = cameraFollowTarget.position + Vector3.up * cameraHeight;
        Vector3 direction = (desiredPosition - targetCenter).normalized;
        float distance = Vector3.Distance(targetCenter, desiredPosition);

        float minCameraDistance = 0.2f; // ✅ The closest the camera can get to the player

        if (Physics.Raycast(targetCenter, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("SmallPlayer"))
            {
                // Move camera in front of hit object, but never closer than minCameraDistance
                float hitDistance = Vector3.Distance(targetCenter, hit.point) - 0.1f;
                hitDistance = Mathf.Max(hitDistance, minCameraDistance); // Ensure it doesn't go closer than min distance
                desiredPosition = targetCenter + direction * hitDistance;
            }
        }

        // Apply final camera position
        playerCamera.transform.position = desiredPosition;
        playerCamera.transform.LookAt(targetCenter);
    }


    private void ToggleClimb()
    {
        if (!isClimbing)
        {
            // Start climbing
            isClimbing = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            // Stop climbing
            isClimbing = false;
            rb.useGravity = true;
        }
    }

    private void ClimbLadder()
    {
        float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");
        Vector3 climbDirection = Vector3.up * vertical;

        rb.linearVelocity = climbDirection * climbSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
            currentLadder = other;
            ladderInteractText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == currentLadder)
        {
            nearLadder = false;
            currentLadder = null;
            ladderInteractText.gameObject.SetActive(false);
            if (isClimbing)
            {
                ToggleClimb(); // stop climbing when leaving ladder
            }
        }
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


