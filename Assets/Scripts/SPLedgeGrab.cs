using UnityEngine;

public class SPLedgeGrab : MonoBehaviour
{
    [Header("Probes")]
    public Transform frontProbe;
    public Transform topProbe;
    public Transform edgeProbe;

    [Header("Detection Ranges")]
    public float frontDistance = 0.6f;
    public float topDistance = 0.2f;
    public float edgeDistanceDown = 1.4f;

    [Header("Ledge Settings")]
    public float ledgeForwardOffset = 0.3f;
    public float ledgeUpOffset = 0.1f;
    public float grabSpeed = 0.05f;

    private CharacterController controller;
    private SPMovementNET movement;

    private bool isGrabbing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<SPMovementNET>();
    }

    void Update()
    {
        if (isGrabbing) return;

        TryLedgeGrab();
    }

    void TryLedgeGrab()
    {
        if (controller.isGrounded) return;

        if (movement == null) return;

        // FRONT wall check
        if (!Physics.Raycast(frontProbe.position, transform.forward, frontDistance))
            return;

        // TOP clearance check
        if (Physics.Raycast(topProbe.position, Vector3.up, topDistance))
            return;

        // EDGE find ledge surface
        RaycastHit hit;
        if (!Physics.Raycast(edgeProbe.position, Vector3.down, out hit, edgeDistanceDown))
            return;

        StartCoroutine(GrabLedge(hit));
    }

    System.Collections.IEnumerator GrabLedge(RaycastHit hit)
    {
        isGrabbing = true;

        // IMPORTANT: stop movement + gravity system
        movement.DisableMovement();

        Vector3 ledgePosition =
            hit.point +
            (-transform.forward * ledgeForwardOffset) +
            (Vector3.up * ledgeUpOffset);

        // move instantly
        controller.enabled = false;
        transform.position = ledgePosition;
        controller.enabled = true;

        yield return new WaitForSeconds(grabSpeed);

        movement.EnableMovement();

        isGrabbing = false;
    }
}
