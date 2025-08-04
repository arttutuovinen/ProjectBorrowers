using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPNewAnimationController : MonoBehaviour
{
    public string horizontalAxis = "P2Horizontal";
    public string verticalAxis = "P2Vertical";

    public float inputThreshold = 0.1f; // Deadzone
    public float speedSmoothTime = 0.1f;

    private Animator animator;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get left stick input
        float h = Input.GetAxis(horizontalAxis);
        float v = Input.GetAxis(verticalAxis);

        // Calculate magnitude (speed)
        float targetSpeed = new Vector2(h, v).magnitude;

        // Apply deadzone threshold
        if (targetSpeed < inputThreshold)
            targetSpeed = 0f;

        // Smooth the speed value
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

        // Set the Animator parameter
        animator.SetFloat("Speed", currentSpeed);
    }
}
