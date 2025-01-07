using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SPEscapeBar : MonoBehaviour
{
    public Transform smallPlayer;
    public Transform smallPlayerTeleportPosition;
    public Image meterImage;       // The UI Image representing the meter.
    public Image meterBackgroundImage;
    public TextMeshProUGUI escapeText;
    public float fillRate = 0.1f;  // The amount to fill the meter per button press.
    public float decayRate = 0.02f; // The rate at which the meter empties over time.
    public float maxFill = 1f;     // Maximum fill amount for the meter (normalized between 0 and 1).

    private float currentFill = 0f; // Current fill amount of the meter.
    private bool isAtTargetPosition = false;

    void Start()
    {
        meterImage.enabled = false;
        meterBackgroundImage.enabled = false;
        escapeText.enabled = false;
    }
    void Update()
    {
        isAtTargetPosition = Vector3.Distance(smallPlayer.position, smallPlayerTeleportPosition.position) < 0.9f;
        if (isAtTargetPosition)
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
            
            meterImage.fillAmount = currentFill;    
        }
        else
        {
            meterImage.enabled = false;
            meterBackgroundImage.enabled = false;
            escapeText.enabled = false;
        }
    }
}
