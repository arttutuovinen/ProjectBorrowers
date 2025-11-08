using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SPAnimController : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    private CharacterController controller;

    // Animation sync variables
    private float horizontal;
    private float vertical;
    private bool isRunning;
    private bool isJumping;
    private bool isFalling;
    private bool isGrounded;

    // Coyote time for jump
    private float coyoteTime = 0.1f;
    private float lastGroundedTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
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

        // Track grounded state
        isGrounded = controller.isGrounded;

        // Update last grounded time for coyote time
        if (isGrounded)
        {
            lastGroundedTime = Time.time;

            if (isJumping || isFalling)
            {
                // Player landed
                isJumping = false;
                isFalling = false;
            }
        }

        bool canJump = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump input
        if ((Input.GetButtonDown("Jump") || Input.GetButtonDown("P1Jump")) && canJump)
        {
            isJumping = true;
        }

        // Falling detection
        if (!isGrounded)
        {
            if (!isJumping)
                isFalling = true;

            isRunning = false;
        }
        else
        {
            // On ground, normal movement
            isRunning = (horizontal != 0 || vertical != 0);
        }
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsGrounded", isGrounded);
    }

    // Photon network syncing
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRunning);
            stream.SendNext(isJumping);
            stream.SendNext(isFalling);
            stream.SendNext(isGrounded);
        }
        else
        {
            isRunning = (bool)stream.ReceiveNext();
            isJumping = (bool)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
        }
    }
}



