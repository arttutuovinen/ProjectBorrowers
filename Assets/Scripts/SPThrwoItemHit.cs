using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SPThrwoItemHit : MonoBehaviour
{
    public float knockbackForce = 10f;  // Strength of the knockback in the random direction
    public float upwardForce = 5f;  // Strength of the upward force
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    private SmallPlayerMovement playerMovement;
    private float destroyTime = 2f;
    public TextMeshProUGUI spIsStunnedText;

    void Start()
    {
        playerMovement = GetComponent<SmallPlayerMovement>();
        spIsStunnedText.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player got hit by an object tagged "BPThrowItemWeapon"
        if (other.gameObject.CompareTag("BPThrowItemWeapon"))
        {
            Debug.Log("SP was HIT");
            // Calculate a random direction in the XZ plane (horizontal direction)
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            spIsStunnedText.enabled = true;
            Vector3 knockbackDirection = randomDirection * knockbackForce + Vector3.up * upwardForce;
            StartCoroutine(KnockbackCoroutine(knockbackDirection));
            playerMovement.DisableMovement();

            // After 2 seconds, re-enable movement (for example)
            Invoke("ReEnableMovement", knockbackDuration + 3f);
            Destroy(other, destroyTime);
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
        // Reset the knockback state after the duration has ended
        isKnockedBack = false;
    }
    private void ReEnableMovement()
    {
        playerMovement.EnableMovement();
        spIsStunnedText.enabled = false;
    }

}
