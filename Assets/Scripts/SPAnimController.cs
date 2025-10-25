using UnityEngine;

public class SPAnimController : MonoBehaviour
{
    [Header("Animator Parameters")]
    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private string isFallingParam = "IsFalling";
    [SerializeField] private string jumpTriggerParam = "IsJumping";

    [Header("Movement Settings")]
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Animator animator;
   
    private float jumpStartTime;
    [SerializeField] private float fallDelay = 0.2f; // seconds after jump starts


    void Awake()
    {
        animator = GetComponent<Animator>();
       
    }

    void Update()
    {
        UpdateRunParameter();
        UpdateJumpAndFall();
    }

    private void UpdateRunParameter()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool isRunning = Mathf.Abs(h) > movementThreshold || Mathf.Abs(v) > movementThreshold;
        animator.SetBool(isRunningParam, isRunning);
    }

    private void UpdateJumpAndFall()
    {
        bool isGrounded = IsGrounded();

        // Trigger jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger(jumpTriggerParam);
            jumpStartTime = Time.time;
        }

        // Determine if falling
        bool isFallingNow = false;

        // Early fall after jump starts
        if (!isGrounded && Time.time - jumpStartTime >= fallDelay)
        {
            isFallingNow = true;
        }

        // Set animator bool
        animator.SetBool(isFallingParam, isFallingNow);

        // Reset IsFalling when grounded
        if (isGrounded)
        {
            animator.SetBool(isFallingParam, false);
        }
    }

    private bool IsGrounded()
    {
        // Use CheckSphere for 3D ground detection
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // Optional: visualize ground check in editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}

