using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BigPlayerMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float turnSmoothTime = 0.1f;

    [Header("Camera Settings")]
    public Transform cameraTransform; // Camera attached to head
    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;
    public float mouseSensitivity = 100f;
    public float controllerSensitivity = 2f;

    [Header("Crouch Settings")]
    public float crouchSpeed = 1f;
    public float standingHeight = 15f;
    public float crouchingHeight = 7.5f;
    public Vector3 standingCenter = Vector3.zero;
    public Vector3 crouchingCenter = new Vector3(0, -3.75f, 0);
    public float crouchCameraYOffset = -0.5f;
    public float cameraTransitionTime = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float yaw;   // root Y rotation
    private float pitch; // camera pitch
    public float currentSpeed;
    private bool isCrouching = false;
    private float originalCameraLocalY;

    [Header("Door Interaction")]
    public float doorRayDistance = 3f;
    public LayerMask doorLayer;

    private Door lookedAtDoor;

    // Networking
    private float networkYaw = 0f;
    public float rotationLerpSpeed = 8f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        currentSpeed = moveSpeed;
        controller.height = standingHeight;
        controller.center = standingCenter;

        if (cameraTransform != null)
            originalCameraLocalY = cameraTransform.localPosition.y;
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            RotateRoot();
            return;
        }

        HandleCamera();
        RotateRoot();
        HandleMovement();
        ApplyGravity();
        HandleCrouchInput();

        UpdateDoorRay();
        HandleDoorInput();
    }
    
    private void UpdateDoorRay()
    {
        lookedAtDoor = null;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            doorRayDistance,
            doorLayer,
            QueryTriggerInteraction.Collide)) // ✅ triggers only
        {
            lookedAtDoor = hit.collider.GetComponentInParent<Door>();
        }
    }
    private void HandleDoorInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (lookedAtDoor == null) return;

        lookedAtDoor.ToggleDoor();
    }

    private void HandleCamera()
    {
        float mouseX, mouseY;

        if (Mathf.Abs(Input.GetAxis("P2RightStickHorizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("P2RightStickVertical")) > 0.1f)
        {
            mouseX = Input.GetAxis("P2RightStickHorizontal") * controllerSensitivity;
            mouseY = Input.GetAxis("P2RightStickVertical") * controllerSensitivity;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Apply pitch only
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void RotateRoot()
    {
        if (photonView.IsMine)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.Euler(0f, networkYaw, 0f),
                Time.deltaTime * rotationLerpSpeed);
        }
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = transform.TransformDirection(direction);
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
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
                StartCoroutine(AdjustCameraY(originalCameraLocalY + crouchCameraYOffset));
            }
            else
            {
                controller.height = standingHeight;
                controller.center = standingCenter;
                currentSpeed = moveSpeed;
                StartCoroutine(AdjustCameraY(originalCameraLocalY));
            }
        }
    }

    private IEnumerator AdjustCameraY(float targetY)
    {
        float startY = cameraTransform.localPosition.y;
        float elapsed = 0f;

        while (elapsed < cameraTransitionTime)
        {
            float t = elapsed / cameraTransitionTime;
            Vector3 localPos = cameraTransform.localPosition;
            localPos.y = Mathf.Lerp(startY, targetY, t);
            cameraTransform.localPosition = localPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPos = cameraTransform.localPosition;
        finalPos.y = targetY;
        cameraTransform.localPosition = finalPos;
    }

    // Photon networking: sync yaw for remote players
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(yaw);
        }
        else
        {
            networkYaw = (float)stream.ReceiveNext();
        }
    }
}
