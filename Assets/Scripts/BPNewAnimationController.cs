using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPNewAnimationController : MonoBehaviourPun, IPunObservable
{
    [Header("Input Settings")]
    private float horizontal;
    private float vertical;       // Default Unity input: W/S or Up/Down
    public string crouchButton = "Crouch";         // Optional Input Manager button

    private Animator animator;
    private bool isCrouching = false;
    private bool isMoving = false;

    // Synced variables for remote players
    private bool networkCrouch = false;
    private bool networkMove = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleCrouchInput();
            HandleMovementInput();
        }
        else
        {
            // Smoothly update remote player animation states
            isCrouching = networkCrouch;
            isMoving = networkMove;
        }

        UpdateAnimator();
    }

    void HandleCrouchInput()
    {
        // Default crouch key (Left Control)
        if (Input.GetButtonDown(crouchButton) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }
    }

    void HandleMovementInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");
        isMoving = (horizontal != 0 || vertical != 0);
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsMoving", isMoving);
    }

    // Photon network sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCrouching);
            stream.SendNext(isMoving);
        }
        else
        {
            networkCrouch = (bool)stream.ReceiveNext();
            networkMove = (bool)stream.ReceiveNext();
        }
    }
}

