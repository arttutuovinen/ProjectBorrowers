using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SPEscapeBar : MonoBehaviour
{
    public Image meterImage;       // The UI Image representing the meter.
    public Image meterBackgroundImage;
    public TextMeshProUGUI escapeText;
    public float fillRate = 0.1f;  // The amount to fill the meter per button press.
    public float decayRate = 0.02f; // The rate at which the meter empties over time.
    public float maxFill = 1f;     // Maximum fill amount for the meter (normalized between 0 and 1).

    private float currentFill = 0f; // Current fill amount of the meter.
    private bool isTouchingTeleportPosition = false;

    void Start()
    {
        meterImage.enabled = false;
        meterBackgroundImage.enabled = false;
        escapeText.enabled = false;
    }
    void Update()
    {
        if (isTouchingTeleportPosition)
        {
            meterImage.enabled = true;
            meterBackgroundImage.enabled = true;
            escapeText.enabled = true;
            
            // Check for button press.
            if (Input.GetButtonDown("P1Jump"))
            {
                // Increase meter fill.
                currentFill += fillRate;
                currentFill = Mathf.Clamp(currentFill, 0f, maxFill);
            }
            else
            {
                // Gradually reduce the meter fill.
                currentFill -= decayRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0f, maxFill);
            }

            // Update the UI Image fill amount.
            if (meterImage != null)
            {
                meterImage.fillAmount = currentFill;
            }
        }
        else
        {
            meterImage.enabled = false;
            meterBackgroundImage.enabled = false;
            escapeText.enabled = false;
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        // Check if the player has touched the required object.
        if (other.gameObject.CompareTag("SPTeleportLocation"))
        {
            Debug.Log("koskee collideria");
            isTouchingTeleportPosition = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        // Reset when the player leaves the required object.
        if (other.gameObject.CompareTag("SPTeleportLocation"))
        {
            isTouchingTeleportPosition = false;
        }
    }
}
