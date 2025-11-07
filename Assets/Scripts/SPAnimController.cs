using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SPAnimController : MonoBehaviourPun
{
    private Animator animator;
    private CharacterController controller;

    // Animation sync variables
    private float horizontal;
    private float vertical;
    private bool isJumping;
    private bool isFalling;
    private bool isRunning;

    private float coyoteTime = 0.1f;
    private float lastGroundedTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        // 🟢 Only the local player should handle input and update animation state.
        if (photonView.IsMine)
        {
            HandleLocalInput();
        }

        // 🟣 All players (including remote ones) should update their animator from current values
        UpdateAnimator();
    }

    void HandleLocalInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

        // Track grounded state with coyote time
        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        bool isGroundedOrCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump
        if ((Input.GetButtonDown("P1Jump") || Input.GetButtonDown("Jump")) && isGroundedOrCoyote)
        {
            animator.ResetTrigger("IsJumping");
            animator.SetTrigger("IsJumping");
            isJumping = true;
        }

        // Falling
        if (!controller.isGrounded)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                isFalling = true;
            }

            isRunning = false;
        }
        else
        {
            isFalling = false;
            isJumping = false;

            bool moving = (horizontal != 0 || vertical != 0);
            isRunning = moving;
        }
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsFalling", isFalling);
        // Trigger for jump is set locally when happens
    }

    // 🔄 This synchronizes animation variables across the network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send animation parameters
            stream.SendNext(isRunning);
            stream.SendNext(isFalling);
            stream.SendNext(isJumping);
        }
        else
        {
            // Remote player: receive animation parameters
            isRunning = (bool)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            bool remoteJump = (bool)stream.ReceiveNext();

            // Handle jump trigger safely
            if (remoteJump && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                animator.ResetTrigger("IsJumping");
                animator.SetTrigger("IsJumping");
            }
        }
    }
}
