using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPSpring : MonoBehaviour
{
    public float knockbackForce = 10f;  // Strength of the upward knockback
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    public AnimationCurve jumpCurve;

    public void UseSpring()
    {
        Debug.Log("SP used SPRING");
        // Knockback direction is straight up (Y-axis only)
        Vector3 knockbackDirection = Vector3.up * knockbackForce;
        // Start the knockback coroutine
        StartCoroutine(KnockbackCoroutine(knockbackDirection));
    }

    private System.Collections.IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        if (isKnockedBack)
            yield break; // If already knocked back, ignore the call

        isKnockedBack = true;

        float timer = 0f;

        // Move the player upward over the knockback duration
        while (timer < knockbackDuration)
        {
            // Apply upward knockback movement per frame
            transform.position += direction * jumpCurve.Evaluate(timer/knockbackDuration) * (Time.deltaTime);

            // Increment the timer
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset the knockback state after the duration has ended
        isKnockedBack = false;
    }
}
