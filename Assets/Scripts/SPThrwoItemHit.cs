using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPThrwoItemHit : MonoBehaviour
{
    public float knockbackForce = 5f;  // Strength of the knockback in the random direction
    public float upwardForce = 20f;  // Strength of the upward force
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    private SmallPlayerMovement playerMovement;

    private void Start()
    {
        // Get the reference to the SmallPlayerMovement script attached to the player
        playerMovement = GetComponent<SmallPlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player got hit by an object tagged "BPThrowItemWeapon"
        if (other.gameObject.CompareTag("BPThrowItemWeapon"))
        {
            // Calculate a random direction in the XZ plane (horizontal direction)
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            // Add the upward force to the direction
            Vector3 knockbackDirection = randomDirection + Vector3.up * upwardForce;
            // Start the knockback coroutine
            StartCoroutine(KnockbackCoroutine(knockbackDirection));

            // Disable player movement when hit by a throw item weapon
            playerMovement.DisableMovement();

            // After 2 seconds, re-enable movement (for example)
            Invoke("ReEnableMovement", 3f);
        }
    }
    private System.Collections.IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        if (isKnockedBack)
            yield break; // If already knocked back, ignore the call

        isKnockedBack = true;

        float timer = 0f;

        // Move the player over the knockback duration
        while (timer < knockbackDuration)
        {
            // Calculate the amount of knockback movement per frame
            transform.position += direction * (knockbackForce * Time.deltaTime);

            // Increment the timer
            timer += Time.deltaTime;
            yield return null;
        }
    }
    private void ReEnableMovement()
    {
        playerMovement.EnableMovement();
    }

}
