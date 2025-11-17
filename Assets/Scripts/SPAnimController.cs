using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SPAnimController : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    private CharacterController controller;

    // Animation state variables
    private float horizontal;
    private float vertical;
    private bool isJumping;
    private bool isFalling;
    private bool isRunning;
    private bool isGrounded;

    // Jump timing
    private float jumpTimer = 0f;
    public float jumpDuration = 0.5f; // Length of your jump animation

    // Coyote time
    private float coyoteTime = 0.1f;
    private float lastGroundedTime;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleLocalInput();
        }

        UpdateAnimator();
    }

    void HandleLocalInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

        bool groundedNow = controller.isGrounded;

        // Track grounded state with coyote time
        if (groundedNow)
            lastGroundedTime = Time.time;

        isGrounded = groundedNow;

        bool isGroundedOrCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump input
        if ((Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")) && isGroundedOrCoyote && !isJumping)
        {
            isJumping = true;
            isFalling = false;
            jumpTimer = 0f;
        }

        // Update jump timer
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            // End jump after jump animation duration
            if (jumpTimer >= jumpDuration)
            {
                isJumping = false;

                // If still in air, switch to falling
                if (!isGrounded)
                {
                    isFalling = true;
                }
            }
        }

        // Falling detection (only if not jumping)
        if (!isGrounded && !isJumping)
        {
            isFalling = true;
        }

        // Running
        isRunning = (horizontal != 0 || vertical != 0) && isGrounded;
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsGrounded", isGrounded);
    }

    // ------------------------------
    // Photon animation sync
    // ------------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send local animation states
            stream.SendNext(isRunning);
            stream.SendNext(isJumping);
            stream.SendNext(isFalling);
            stream.SendNext(isGrounded);
        }
        else
        {
            // Receive remote animation states
            isRunning = (bool)stream.ReceiveNext();
            isJumping = (bool)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
        }
    }
}


