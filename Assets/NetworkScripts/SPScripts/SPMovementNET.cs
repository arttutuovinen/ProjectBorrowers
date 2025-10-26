using UnityEngine;
using TMPro;
using Unity.Netcode;


public class SPMovementNET : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 40f;
    public float jumpForce = 11f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayers;

    [Header("Camera Settings")]
    public Transform cameraFollowTarget;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;

    [Header("Gravity Settings")]
    public float fallMultiplier = 2.5f;

    [Header("Jump Settings")]
    public float coyoteTime = 0.2f;
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

    // Optional: Animator if you have animations
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (!IsOwner)
        {
            // Disable Rigidbody physics for non-owners to prevent conflicts
            rb.isKinematic = true;
            return;
        }

        playerCamera = GameObject.Find("SmallPlayerCamera")?.GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        ladderInteractText = GameObject.Find("SPinteractLadder")?.GetComponent<TextMeshProUGUI>();
        if (ladderInteractText != null) ladderInteractText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMouseLook();
        HandleCamera();
        HandleInput();
        CheckGrounded();
        HandleJump();
        HandleLadder();

        // Animator updates
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isClimbing) return; // Stop normal physics while climbing

        ApplyMovement();
        ApplyBetterGravity();
    }

    void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

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
        Vector3 targetVelocity = moveInput * moveSpeed;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        Vector3 velocityChange = (targetVelocity - currentVelocity) * acceleration * Time.fixedDeltaTime;

        if (isGrounded)
            rb.linearVelocity += velocityChange;
        else
            rb.linearVelocity += velocityChange * 0.5f;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    void HandleJump()
    {
        if (coyoteCounter > 0f && (Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            coyoteCounter = 0f;
        }
    }

    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance + 0.05f, groundLayers);
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;
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

        Quaternion rotation = Quaternion.Euler(cameraXRotation, cameraYRotation, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, 0, -cameraDistance);
        Vector3 desiredPosition = cameraFollowTarget.position + desiredOffset + Vector3.up * cameraHeight;

        Vector3 targetCenter = cameraFollowTarget.position + Vector3.up * cameraHeight;
        Vector3 direction = (desiredPosition - targetCenter).normalized;
        float distance = Vector3.Distance(targetCenter, desiredPosition);
        float minCameraDistance = 0.2f;

        if (Physics.Raycast(targetCenter, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("SmallPlayer"))
            {
                float hitDistance = Mathf.Max(Vector3.Distance(targetCenter, hit.point) - 0.1f, minCameraDistance);
                desiredPosition = targetCenter + direction * hitDistance;
            }
        }

        playerCamera.transform.position = desiredPosition;
        playerCamera.transform.LookAt(targetCenter);
    }

    void HandleLadder()
    {
        if (!nearLadder) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("P1Interact"))
            ToggleClimb();

        if (isClimbing)
        {
            float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");
            rb.linearVelocity = Vector3.up * vertical * climbSpeed;
        }
    }

    private void ToggleClimb()
    {
        isClimbing = !isClimbing;
        rb.useGravity = !isClimbing;
        if (isClimbing) rb.linearVelocity = Vector3.zero;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("MoveSpeed", moveInput.magnitude);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsClimbing", isClimbing);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
            currentLadder = other;
            if (ladderInteractText != null) ladderInteractText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other == currentLadder)
        {
            nearLadder = false;
            currentLadder = null;
            if (ladderInteractText != null) ladderInteractText.gameObject.SetActive(false);
            if (isClimbing) ToggleClimb();
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}


