using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPAnimController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    private bool isGrounded;

    // Coyote time
    private float coyoteTime = 0.1f;
    private float lastGroundedTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // Check grounded with sphere for reliability
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ~0, QueryTriggerInteraction.Ignore);
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        bool isGroundedOrCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        // Jump input
        if ((Input.GetButtonDown("P1Jump") || Input.GetButtonDown("Jump")) && isGroundedOrCoyote)
        {
            animator.ResetTrigger("IsJumping");
            animator.SetTrigger("IsJumping");
        }

        // Determine horizontal movement
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMoving = horizontalVelocity.magnitude > 0.1f;

        animator.SetBool("IsRunning", isGrounded && isMoving);

        // Falling check
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
        animator.SetBool("IsFalling", isFalling);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
