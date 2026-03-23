using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class BPCompass : MonoBehaviourPun
{
    [Header("References")]
    public GameObject bpCompass;
    public GameObject bpArrow;
    public Transform arrow;        // Only the arrow rotates

    private BPFpAnimationController bpfpAnimation;

    [Header("Settings")]
    public float compassDuration = 5f;
    private Quaternion initialArrowRotation;
    private Coroutine compassRoutine;

    private void Start()
    {
        bpfpAnimation = GetComponent<BPFpAnimationController>();
        initialArrowRotation = arrow.localRotation;
        if (bpCompass != null)
            bpCompass.SetActive(false);
        if (bpArrow != null)
            bpArrow.SetActive(false);
    }

    public void UseCompass()
    {
        if (!photonView.IsMine) return;

        GameObject[] allSPs = GameObject.FindGameObjectsWithTag("SmallPlayer");
        if (allSPs.Length == 0) return;

        GameObject targetSP = allSPs[Random.Range(0, allSPs.Length)];

        // Activate visuals + animation
        bpCompass.SetActive(true);
        bpArrow.SetActive(true);
        bpfpAnimation.PlayCompassAnimation();

        // Prevent stacking coroutines
        if (compassRoutine != null)
            StopCoroutine(compassRoutine);

        compassRoutine = StartCoroutine(UpdateCompass(targetSP));
    }

    private IEnumerator UpdateCompass(GameObject targetSP)
    {
        float elapsed = 0f;

        while (elapsed < compassDuration && targetSP != null)
        {
            // Direction from player to target (ignore height)
            Vector3 direction = targetSP.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                // Convert to local space for stable rotation
                Vector3 localDir = transform.InverseTransformDirection(direction);

                float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

                // Rotate only the arrow
                arrow.localRotation = initialArrowRotation * Quaternion.Euler(0f, angle, 0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset everything after duration
        bpCompass.SetActive(false);
        bpArrow.SetActive(false);
        bpfpAnimation.ResetCompassAnimation();

        compassRoutine = null;
    }
}



