using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPEscapeBar : MonoBehaviour
{
    public Image meterImage;       // The UI Image representing the meter.
    public float fillRate = 0.1f;  // The amount to fill the meter per button press.
    public float decayRate = 0.02f; // The rate at which the meter empties over time.
    public float maxFill = 1f;     // Maximum fill amount for the meter (normalized between 0 and 1).

    private float currentFill = 0f; // Current fill amount of the meter.

    void Update()
    {
        // Check for button press.
        if (Input.GetButtonDown("P1Jump"))
        {
            // Increase meter fill.
            currentFill += fillRate;
            currentFill = Mathf.Clamp(currentFill, 0f, maxFill); // Ensure value stays within range.
        }
        else
        {
            // Gradually reduce the meter fill.
            currentFill -= decayRate * Time.deltaTime;
            currentFill = Mathf.Clamp(currentFill, 0f, maxFill); // Ensure value doesn't go below 0.
        }

        // Update the UI Image fill amount.
        if (meterImage != null)
        {
            meterImage.fillAmount = currentFill;
        }
    }
}
