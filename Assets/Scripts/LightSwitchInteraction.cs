using UnityEngine;
using System.Collections;
public class LightSwitchInteraction : MonoBehaviour
{
   public Light targetLight;            // The light to toggle
    public Transform switchHandle;       // The switch part to rotate
    public float rotationAngle = 45f;    // How much to rotate when toggled
    public float rotationSpeed = 5f;     // Smooth rotation speed

    private bool isOn;
    private Quaternion originalRotation; // Store the starting rotation
    private Quaternion targetRotation;

    private void Start()
    {
        if (switchHandle == null)
            switchHandle = this.transform; // fallback

        originalRotation = switchHandle.localRotation;

        // Sync state
        if (targetLight != null)
            isOn = targetLight.enabled;

        targetRotation = isOn
            ? originalRotation * Quaternion.Euler(rotationAngle, 0f, 0f)
            : originalRotation;

        // Ensure switch handle matches light state at start
        switchHandle.localRotation = targetRotation;
    }

    public void Interact()
    {
        isOn = !isOn;

        if (targetLight != null)
            targetLight.enabled = isOn;

        targetRotation = isOn
            ? originalRotation * Quaternion.Euler(rotationAngle, 0f, 0f)
            : originalRotation;

        StopAllCoroutines();
        StartCoroutine(RotateSwitch());
    }

    private IEnumerator RotateSwitch()
    {
        while (Quaternion.Angle(switchHandle.localRotation, targetRotation) > 0.1f)
        {
            switchHandle.localRotation = Quaternion.Slerp(
                switchHandle.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
            yield return null;
        }

        switchHandle.localRotation = targetRotation;
    }
}


