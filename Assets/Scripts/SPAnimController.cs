using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SPAnimController : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    private CharacterController controller;

    // Animation sync variables
    private float horizontal;
    private float vertical;
    private bool isJumping;
    private bool isFalling;
    private bool isRunning;
    private bool isGrounded;

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

        isGrounded = controller.isGrounded;

        // Track grounded state with coyote time
        if (isGrounded)
            lastGroundedTime = Time.time;

        bool isGroundedOrCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump
        if ((Input.GetButtonDown("P1Jump") || Input.GetButtonDown("Jump")) && isGroundedOrCoyote)
        {
            isJumping = true;
        }

        // Falling
        if (!isGrounded && !isJumping)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        isRunning = (horizontal != 0 || vertical != 0) && isGrounded;

        // Reset jump if grounded
        if (isGrounded && isJumping)
            isJumping = false;
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsGrounded", isGrounded);
    }

    // ------------------------------
    // Network sync
    // ------------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send local animation bools
            stream.SendNext(isRunning);
            stream.SendNext(isJumping);
            stream.SendNext(isFalling);
            stream.SendNext(isGrounded);
        }
        else
        {
            // Receive remote bools
            isRunning = (bool)stream.ReceiveNext();
            isJumping = (bool)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
        }
    }
}

