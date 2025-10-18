using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPAnimController : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    // Small buffer so jump can still trigger right after leaving ground
    private float coyoteTime = 0.1f;  
    private float lastGroundedTime;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        controller = GetComponentInParent<CharacterController>(); 
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("P1Horizontal");
        float vertical = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("P1Vertical");

        // Track grounded state with coyote time
        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        bool isGroundedOrCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump input
        if (Input.GetButtonDown("P1Jump") || Input.GetButtonDown("Jump") && isGroundedOrCoyote)
        {
            animator.ResetTrigger("IsJumping"); // ensures clean trigger
            animator.SetTrigger("IsJumping");
        }

        // Falling check (don’t override jump immediately)
        if (!controller.isGrounded)
        {
            // Only set falling if not already in jump animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                animator.SetBool("IsFalling", true);
            }

            // Stop running while in air
            animator.SetBool("IsRunning", false);
        }
        else
        {
            animator.SetBool("IsFalling", false);

            // Only check running when grounded
            bool isMoving = (horizontal != 0 || vertical != 0);
            animator.SetBool("IsRunning", isMoving);
        }
    }
}
