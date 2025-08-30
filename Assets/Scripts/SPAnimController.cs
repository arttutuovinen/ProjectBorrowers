using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPAnimController : MonoBehaviour
{
   private Animator animator;
    private CharacterController controller;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        controller = GetComponentInParent<CharacterController>(); 
        // Assuming controller is on parent object
    }

    void Update()
{
    float horizontal = Input.GetAxisRaw("P1Horizontal");
    float vertical = Input.GetAxisRaw("P1Vertical");

    // Jump input
    if (Input.GetButtonDown("P1Jump") && controller.isGrounded)
    {
        animator.SetTrigger("IsJumping");
    }

    // Falling check
    if (!controller.isGrounded)
    {
        animator.SetBool("IsFalling", true);
        // Don’t update IsRunning while in air
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
