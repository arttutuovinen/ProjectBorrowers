using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPNewAnimationController : MonoBehaviour
{
    public string horizontalAxis = "P2Horizontal";
    public string verticalAxis = "P2Vertical";
    public string crouchButton = "P2Crouch";

    public float inputThreshold = 0.1f;
    public float speedSmoothTime = 0.1f;

    private Animator animator;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;
    private bool isCrouching = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleCrouchInput();
        HandleMovementInput();
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
        // Set Animator values AFTER logic is stable
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetFloat("Speed", currentSpeed);
    }
}
