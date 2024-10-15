using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlayerAnimation : MonoBehaviour
{
    public Animator animator;
    private bool canGrab = true;  // A flag to check if the player can grab
    public float cooldownTime = 3f;  // Cooldown duration in seconds
    private float nextGrabTime = 0f;  // Time when the player can grab again
    private bool isFlySwapperActive = false; // A flag to track if FlySwapper is active
    public GameObject flySwatter;

    public string smallPlayerTag = "SmallPlayer";  // Tag for Small Player

    void Start()
    {
        flySwatter.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if cooldown is over
        if (Time.time >= nextGrabTime)
        {
            canGrab = true;
        }

        // Check if the "P2PickUp" button is pressed and cooldown is over
        if (Input.GetButtonDown("P2PickUp") && canGrab && !isFlySwapperActive)
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

    public void FlySwatter()
    {
        flySwatter.SetActive(true);
        Debug.Log("flySwatter WORKS");
        animator.SetTrigger("flySwatterTrigger");
        isFlySwapperActive = true;

        // Start a coroutine to reset the state after the animation
        StartCoroutine(EndFlySwatterAfterDelay());
    }

    private IEnumerator EndFlySwatterAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // Adjust this to match the length of your FlySwatter animation
        EndFlySwatter();
    }

    // Method to reset the FlySwapper state, allowing other actions again
    public void EndFlySwatter()
    {
        flySwatter.SetActive(false);
        isFlySwapperActive = false;
        Debug.Log("FlySwatter action ended, player can now use P2PickUp again.");
    }

    // This method detects if the flySwatter hits a SmallPlayer
    private void OnTriggerEnter(Collider other)
    {
        // Check if the flySwatter is active and collides with a SmallPlayer
        if (isFlySwapperActive && other.CompareTag(smallPlayerTag))
        {
            Debug.Log("flySwatter hit a SmallPlayer!");
            CaughtAnimation();
        }
    }
}
