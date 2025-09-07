using UnityEngine;

public class CeilingFanRotation : MonoBehaviour
{
    [Tooltip("The rotating part (child object) that actually spins")]
    public Transform rotatingFan;

    [Tooltip("Degrees per second around Y axis")]
    public float rotationSpeed = 50f;

    private Transform playerTransform;
    private CharacterController playerController;
    private bool playerOnFan = false;

    private Quaternion lastRotation;

    void Start()
    {
        if (rotatingFan == null)
        {
            Debug.LogError("FanRotator: Please assign the rotating fan child!");
            enabled = false;
            return;
        }

        lastRotation = rotatingFan.rotation;
    }

    void Update()
    {
        // Rotate the fan
        rotatingFan.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        if (playerOnFan && playerTransform != null && playerController != null)
        {
            // Calculate how much the fan rotated since last frame
            Quaternion deltaRotation = rotatingFan.rotation * Quaternion.Inverse(lastRotation);

            // Apply that rotation to the player's position (around the fan's center)
            Vector3 offset = playerTransform.position - rotatingFan.position;
            offset = deltaRotation * offset;

            Vector3 newPosition = rotatingFan.position + offset;
            Vector3 movement = newPosition - playerTransform.position;

            // Move the CharacterController with this delta
            playerController.Move(movement);
        }

        lastRotation = rotatingFan.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SmallPlayer"))
        {
            playerTransform = other.transform;
            playerController = other.GetComponent<CharacterController>();
            playerOnFan = (playerController != null);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SmallPlayer"))
        {
            playerOnFan = false;
            playerTransform = null;
            playerController = null;
        }
    }
}
