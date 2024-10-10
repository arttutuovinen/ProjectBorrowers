using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BPFlashbanged : MonoBehaviour
{
    public Image flashbangedImage; // The UI Image to change the alpha value of.
    public float fadeDuration = 3f;
    public float initialDelay = 3f;

    private void Start()
    {
        // Set the initial alpha of the image to 0 (completely transparent)
        SetImageAlpha(0f);
    }

    // Detect when the player enters the FlashbangEffectArea's collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the tag "FlashbangEffectArea"
        if (other.CompareTag("FlashbangEffectArea"))
        {
            SetImageAlpha(1f);
            StartCoroutine(StartFadeOutWithDelay());
        }
    }
    // Coroutine to handle the delay and then start fading out
    private IEnumerator StartFadeOutWithDelay()
    {
        // Wait for the specified delay time before starting the fade-out
        yield return new WaitForSeconds(initialDelay);
        Debug.Log("Starting fade-out after delay.");

        // Start the fade-out coroutine
        StartCoroutine(FadeOut());
    }
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color originalColor = flashbangedImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            SetImageAlpha(newAlpha);
            yield return null; // Wait for the next frame
        }

        // Ensure the alpha is set to 0 at the end of the fade-out
        SetImageAlpha(0f);
        Debug.Log("Image alpha set to 0 after fading out.");
    }

    // Method to set the alpha of the image
    private void SetImageAlpha(float alpha)
    {
        Color color = flashbangedImage.color;
        color.a = alpha;
        flashbangedImage.color = color;
    }
    

}
