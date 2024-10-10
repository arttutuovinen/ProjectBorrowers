using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlayerAnimation : MonoBehaviour
{
    public Animator animator;
    private bool canGrab = true;  // A flag to check if the player can grab
    public float cooldownTime = 3f;  // Cooldown duration in seconds
    private float nextGrabTime = 0f;  // Time when the player can grab again

    // Update is called once per frame
    void Update()
    {
        // Check if cooldown is over
        if (Time.time >= nextGrabTime)
        {
            canGrab = true;
        }

        // Check if the "P2PickUp" button is pressed and cooldown is over
        if (Input.GetButtonDown("P2PickUp") && canGrab)
        {
            // Trigger the "Grab" animation
            animator.SetTrigger("grabTrigger");
            Debug.Log("Animation works");

            // Start cooldown
            canGrab = false;
            nextGrabTime = Time.time + cooldownTime;
        }
    }

    public void CaughtAnimation()
    {
        Debug.Log("PLAY GRAB ANIMATION");
        animator.SetTrigger("caughtTrigger");
    }

    public void ReleaseAnimation()
    {
        Debug.Log("RELEASED");
        animator.SetTrigger("releaseTrigger");
    }
}
