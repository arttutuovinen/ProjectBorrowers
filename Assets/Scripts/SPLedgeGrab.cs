using UnityEngine;

public class SPLedgeGrab : MonoBehaviour
{
    [Header("Probes")]
    public Transform frontProbe;
    public Transform topProbe;
    public Transform edgeProbe;

    [Header("Detection Ranges")]
    public float frontDistance = 0.8f;
    public float topDistance = 0.15f;
    public float edgeDistanceDown = 1.4f;

    [Header("Ledge Settings")]
    public float ledgeForwardOffset = 0.3f;
    public float ledgeUpOffset = 0.3f;
    public float grabSpeed = 0.05f;

    [Header("Sticky Feel")]
    public float ledgeStickTime = 0.15f; // 👈 NEW (grace window)

    private CharacterController controller;
    private SPMovementNET movement;

    private bool isGrabbing = false;

    private bool ledgeBuffered = false;
    private float bufferTimer = 0f;
    private RaycastHit bufferedHit;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<SPMovementNET>();
    }

    void Update()
    {
        if (isGrabbing) return;

        TryDetectLedge();
        HandleStickyBuffer();
    }

    void DrawDebugRay(Vector3 origin, Vector3 dir, float dist, bool hit)
    {
        Debug.DrawRay(origin, dir * dist, hit ? Color.green : Color.red);
    }

    void TryDetectLedge()
    {
        if (controller.isGrounded) return;
        if (movement == null) return;

        // FRONT WALL
        bool frontHit = Physics.Raycast(frontProbe.position, transform.forward, frontDistance);
        DrawDebugRay(frontProbe.position, transform.forward, frontDistance, frontHit);

        if (!frontHit)
            return;

        // TOP CLEAR
        bool topHit = Physics.Raycast(topProbe.position, Vector3.up, topDistance);
        DrawDebugRay(topProbe.position, Vector3.up, topDistance, topHit);

        if (topHit)
            return;

        // EDGE FIND
        RaycastHit hit;
        bool edgeHit = Physics.Raycast(edgeProbe.position, Vector3.down, out hit, edgeDistanceDown);
        DrawDebugRay(edgeProbe.position, Vector3.down, edgeDistanceDown, edgeHit);

        if (!edgeHit)
            return;

        // BUFFER (sticky)
        ledgeBuffered = true;
        bufferTimer = ledgeStickTime;
        bufferedHit = hit;
    }

    void HandleStickyBuffer()
    {
        if (!ledgeBuffered) return;

        bufferTimer -= Time.deltaTime;

        if (bufferTimer <= 0f)
        {
            ledgeBuffered = false;
            return;
        }

        TryLedgeGrab(bufferedHit);
    }

    void TryLedgeGrab(RaycastHit hit)
    {
        if (isGrabbing) return;

        isGrabbing = true;
        ledgeBuffered = false;

        movement.DisableMovement();

        Vector3 ledgePosition =
            hit.point +
            (-transform.forward * ledgeForwardOffset) +
            (Vector3.up * ledgeUpOffset);

        controller.enabled = false;
        transform.position = ledgePosition;
        controller.enabled = true;

        Invoke(nameof(EnableMovementBack), grabSpeed);
    }

    void EnableMovementBack()
    {
        movement.EnableMovement();
        isGrabbing = false;
    }
}
