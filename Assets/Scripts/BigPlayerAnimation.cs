using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlayerAnimation : MonoBehaviour
{
   public Animator animator;
    private bool canGrab = true;          // Check if the player can grab
    public float cooldownTime = 3f;       // Cooldown duration in seconds
    private float nextGrabTime = 0f;      // Time when the player can grab again

    private bool isFlySwapperActive = false; // Track if FlySwapper is active
    private bool canCatch = false;           // 🆕 Window that controls when catching is allowed

    public GameObject flySwatter;
    public string smallPlayerTag = "SmallPlayer"; // Tag for Small Player

    void Start()
    {
        flySwatter.SetActive(false);
    }

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
            Debug.Log("Grab animation triggered");

            // Start cooldown
            canGrab = false;
            nextGrabTime = Time.time + cooldownTime;
        }
    }

    public void CaughtAnimation()
    {
        Debug.Log("PLAY CAUGHT ANIMATION");
        animator.SetTrigger("caughtTrigger");
    }

    public void ReleaseAnimation()
    {
        Debug.Log("RELEASED");
        animator.SetTrigger("releaseTrigger");
    }

    // Called (for example) via animation event or another script
    public void FlySwatter()
    {
        flySwatter.SetActive(true);
        Debug.Log("flySwatter activated");
        animator.SetTrigger("flySwatterTrigger");
        isFlySwapperActive = true;

        // 🆕 Start the coroutine with the catch window logic
        StartCoroutine(FlySwatterCatchWindow());
    }

    // 🆕 Coroutine with a short "catch window"
    private IEnumerator FlySwatterCatchWindow()
    {
        // Wait a short moment before allowing a catch (prevents instant caught)
        yield return new WaitForSeconds(0.3f);
        canCatch = true;  // now hits will count
        Debug.Log("Catch window OPEN");

        // Keep the window open for ~1.7 seconds (adjust as needed)
        yield return new WaitForSeconds(1.7f);
        canCatch = false;
        Debug.Log("Catch window CLOSED");

        EndFlySwatter();
    }

    public void EndFlySwatter()
    {
        flySwatter.SetActive(false);
        isFlySwapperActive = false;
        Debug.Log("FlySwatter ended, player can grab again.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 🆕 Only register hits during the active catch window
        if (isFlySwapperActive && canCatch && other.CompareTag(smallPlayerTag))
        {
            Debug.Log("flySwatter hit a SmallPlayer!");
            CaughtAnimation();
        }
    }
}
