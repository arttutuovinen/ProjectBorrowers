using UnityEngine;
using System.Collections;

public class SPPullTarget : MonoBehaviour
{
     private CharacterController controller;
    private bool isBeingPulled = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void PullTowards(Transform source, float force, float duration)
    {
        if (!isBeingPulled)
            StartCoroutine(PullRoutine(source, force, duration));
    }

    private IEnumerator PullRoutine(Transform source, float force, float duration)
    {
        isBeingPulled = true;
        float timer = 0f;

        while (timer < duration)
        {
            if (source == null) break;

            Vector3 direction = (source.position - transform.position).normalized;
            controller.Move(direction * force * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        isBeingPulled = false;
    }
}
