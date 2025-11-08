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
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleLocalInput();
        }

        // ✅ Detect early landing: if landed before jump animation finishes
        if (isGrounded && animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            string target = (isRunning ? "Run" : "Idle");
            animator.CrossFade(target, 0.1f);
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
            isFalling = false;
        }

        // Falling
        if (!isGrounded)
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
            isRunning = (horizontal != 0 || vertical != 0);
        }
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsGrounded", isGrounded);

        if (isJumping)
        {
            animator.SetTrigger("IsJumping");
            if (!photonView.IsMine) isJumping = false; // remote reset
        }
    }

    // 🔁 Sync animation states across network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRunning);
            stream.SendNext(isFalling);
            stream.SendNext(isJumping);
            stream.SendNext(isGrounded);
        }
        else
        {
            isRunning = (bool)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            bool remoteJump = (bool)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();

            if (remoteJump && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                animator.ResetTrigger("IsJumping");
                animator.SetTrigger("IsJumping");
            }
        }
    }
}




