using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPNewAnimationController : MonoBehaviourPun, IPunObservable
{
    [Header("Animator")]
    public Animator animator;   // MUST be assigned in inspector (BG animator)

    [Header("Input Settings")]
    private float horizontal;
    private float vertical;
    public string crouchButton = "Crouch";

    private bool isCrouching = false;
    private bool isMoving = false;

    // Network synced values
    private bool networkCrouch = false;
    private bool networkMove = false;

    void Awake()
    {
        // Safety check
        if (animator == null)
        {
            Debug.LogError("BPNewAnimationController: Animator reference missing! Assign BG Animator in inspector.");
        }
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
            // Apply synced values
            isCrouching = networkCrouch;
            isMoving = networkMove;
        }

        UpdateAnimator();
    }

    void HandleCrouchInput()
    {
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
        if (animator == null) return;

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsMoving", isMoving);
    }

    // Photon sync
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

