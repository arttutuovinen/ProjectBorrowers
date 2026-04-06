using UnityEngine;

public class SPRescueTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SPRescueHandler rescue = other.GetComponent<SPRescueHandler>();
        SPCaptureHandler sp = other.GetComponent<SPCaptureHandler>();

        if (rescue == null || sp == null) return;
        if (sp.IsCaptured() || sp.IsJailed()) return;

        // If ANY jailed players exist
        if (SPCaptureHandler.JailedSPs.Count > 0)
        {
            rescue.ShowRelease();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        SPRescueHandler rescue = other.GetComponent<SPRescueHandler>();
        if (rescue == null) return;

        rescue.HideRelease();
    }
}
