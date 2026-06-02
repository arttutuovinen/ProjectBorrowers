using UnityEngine;

public class SPLedgeGrab : MonoBehaviour
{
    [Header("References")]
    public Transform frontProbe;
    public Transform edgeProbe;
    public Transform topProbe;

    [Header("Ray Distances")]
    public float frontCheckDistance = 0.5f;
    public float topCheckDistance = 0.5f;
    public float edgeCheckDistance = 2f;

    public Transform groundCheck;

    private SPMovementNET movement;
    private CharacterController controller;

    private void Start()
    {
        movement = GetComponent<SPMovementNET>();
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (movement == null)
            return;

        // Only allow ledge grabbing while in the air
        if (movement.CurrentState != PlayerState.Jumping)
            return;

        CheckForLedge();
    }

    private void CheckForLedge()
    {
        // Must hit a wall in front
        if (!Physics.Raycast(frontProbe.position, transform.forward, out RaycastHit frontHit, frontCheckDistance))
            return;

        // Must have empty space above
        if (Physics.Raycast(topProbe.position, transform.forward, topCheckDistance))
            return;

        // Must find a top surface
        if (!Physics.Raycast(edgeProbe.position, Vector3.down, out RaycastHit edgeHit, edgeCheckDistance))
            return;

        GrabLedge(edgeHit.point);
    }

    private void GrabLedge(Vector3 ledgePoint)
    {
        movement.SetState(PlayerState.LedgeGrab);

        controller.enabled = false;

        Vector3 offset = transform.position - groundCheck.position;
        transform.position = ledgePoint + offset;

        controller.enabled = true;

        movement.SetState(PlayerState.Grounded);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (frontProbe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(frontProbe.position, transform.forward * frontCheckDistance);
        }

        if (topProbe != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(topProbe.position, transform.forward * topCheckDistance);
        }

        if (edgeProbe != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(edgeProbe.position, Vector3.down * edgeCheckDistance);
        }
    }
#endif
}
