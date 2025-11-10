using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPNewAnimationController : MonoBehaviourPun, IPunObservable
{
    [Header("Input Settings")]
    public string horizontalAxis = "P2Horizontal";
    public string verticalAxis = "P2Vertical";
    public string crouchButton = "P2Crouch";

    [Header("Movement Settings")]
    public float inputThreshold = 0.1f;
    public float speedSmoothTime = 0.1f;

    private Animator animator;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;
    private bool isCrouching = false;

    // Synced variables for remote players
    private float networkSpeed = 0f;
    private bool networkCrouch = false;

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
            // Smooth transitions for remote players
            currentSpeed = Mathf.Lerp(currentSpeed, networkSpeed, Time.deltaTime * 8f);
            isCrouching = networkCrouch;
        }

        UpdateAnimator();
    }

    void HandleCrouchInput()
    {
        if (Input.GetButtonDown(crouchButton))
        {
            isCrouching = !isCrouching;
        }
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxis(horizontalAxis);
        float v = Input.GetAxis(verticalAxis);
        float targetSpeed = new Vector2(h, v).magnitude;

        // Apply threshold
        if (targetSpeed < inputThreshold)
            targetSpeed = 0f;

        // Smooth speed
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetFloat("Speed", currentSpeed);
    }

    // Sync animation data across network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send local player animation data
            stream.SendNext(currentSpeed);
            stream.SendNext(isCrouching);
        }
        else
        {
            // Receive and apply data for remote players
            networkSpeed = (float)stream.ReceiveNext();
            networkCrouch = (bool)stream.ReceiveNext();
        }
    }
}

