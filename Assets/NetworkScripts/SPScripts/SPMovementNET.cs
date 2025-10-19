using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity = moveInput * moveSpeed;
            Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // Apply acceleration-based movement
            rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);

            // Rotate toward move direction
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump"))
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
         // Use a small sphere to detect ground contact
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ~0, QueryTriggerInteraction.Ignore);
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
        }
    }
}

