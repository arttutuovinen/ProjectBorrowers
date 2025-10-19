using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SPMovementNET : NetworkBehaviour
{
    public float moveSpeed = 7f; 
    public float climbSpeed = 8f; 
    public float gravity = -30f; 
    public float jumpHeight = 3f; 
    public GameObject playerCamera; 

    public float turnSmoothTime = 0.1f; 
    private bool canMove = true; 

    public Transform cameraTransform; 
    public Transform cameraFollowTarget; 

    public float mouseSensitivity = 300f; 
    public float controllerSensitivity = 2f; 
    public float distanceFromPlayer = 3f; 
    public float minVerticalAngle = -30f; 
    public float maxVerticalAngle = 60f; 

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float turnSmoothVelocity;

    private float yaw; 
    private float pitch; 

    private bool isClimbing = false; 
    private bool nearLadder = false; 
    private Collider ladder; 

    public bool isTreasureCollected = false;

    public TextMeshProUGUI ladderInteractText;
    public TextMeshProUGUI itemInteractText;

    public ParticleSystem movementParticles;
    public float maxEmissionRate = 20f; 
    public float emissionChangeSpeed = 10f; 
    private ParticleSystem.EmissionModule emissionModule;

    [Header("Ground Check")]
    public Transform groundCheck; // 🔹 assign an empty child at feet
    public LayerMask groundLayer;
    public float groundDistance = 0.1f;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();

        // 🔹 Ensure CharacterController sits on ground if pivot is at feet
        controller.center = new Vector3(0, controller.height / 2f, 0);

        Debug.Log($"Player spawned. IsOwner={IsOwner}, LocalClientId={NetworkManager.Singleton.LocalClientId}");

        if (IsOwner)
        {
            // 🔹 Nudge down slightly to ensure grounded on spawn
            controller.Move(Vector3.down * 0.05f);

            // 🔹 Find and assign the separate camera
            if (playerCamera == null)
            {
                GameObject foundCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (foundCamera != null)
                    playerCamera = foundCamera;
                else
                    Debug.LogWarning("No MainCamera found in scene for player to attach!");
            }

            if (playerCamera != null)
            {
                playerCamera.SetActive(true);
                cameraTransform = playerCamera.transform;
                cameraFollowTarget = this.transform;
                cameraTransform.SetParent(null); // detach camera for smooth movement
            }
        }
        else
        {
            if (playerCamera != null)
                playerCamera.SetActive(false);
        }

        if (ladderInteractText != null)
            ladderInteractText.gameObject.SetActive(false);

        emissionModule = movementParticles.emission;
        emissionModule.rateOverTime = 0f; 
    }

    private void Update()
    {
        if (!IsOwner) return; // 🔹 Only owner controls movement

        // 🔹 Ground check using small sphere at feet
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (isClimbing)
            ClimbLadder();
        else
        {
            Move();
            ApplyGravity();
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return; // 🔹 Only owner controls camera
        ControlCamera();
    }

    public void EnableMovement() => canMove = true;
    public void DisableMovement() => canMove = false;

    public void Move()
    {
        if (!canMove) return;

        float horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // 🔹 Particle emission logic
        float targetRate = (isGrounded && direction.magnitude >= 0.1f) ? maxEmissionRate : 0f;
        float newRate = Mathf.Lerp(emissionModule.rateOverTime.constant, targetRate, Time.deltaTime * emissionChangeSpeed);
        emissionModule.rateOverTime = newRate;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        if ((Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (nearLadder && (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("P1Interact")))
        {
            isClimbing = true;
            velocity.y = 0f;
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ClimbLadder()
    {
        velocity.y = 0f;
        float vertical = Input.GetAxisRaw("P1Vertical") + Input.GetAxisRaw("Vertical");
        controller.Move(new Vector3(0, vertical, 0).normalized * climbSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("P1Interact"))
            isClimbing = false;
    }

    private void ControlCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        Vector3 desiredDir = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back;
        Vector3 desiredPos = cameraFollowTarget.position + desiredDir * distanceFromPlayer;

        RaycastHit hit;
        if (Physics.Raycast(cameraFollowTarget.position, desiredPos - cameraFollowTarget.position, out hit, distanceFromPlayer))
            desiredPos = cameraFollowTarget.position + (desiredPos - cameraFollowTarget.position).normalized * Mathf.Max(hit.distance - 0.3f, 0.1f);

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPos, Time.deltaTime * 15f);
        cameraTransform.LookAt(cameraFollowTarget.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
            ladder = other;
            if (ladderInteractText != null)
                ladderInteractText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == ladder)
        {
            nearLadder = false;
            ladder = null;
            isClimbing = false;
            if (ladderInteractText != null)
                ladderInteractText.gameObject.SetActive(false);
        }
    }
}

