using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BPInkSplatUI : MonoBehaviour
{
    public Image oilSplat;

    private Coroutine currentRoutine;

    void Start()
    {
        if (oilSplat != null)
            oilSplat.gameObject.SetActive(false);
    }

    public void ShowInkSplat()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(InkSplatRoutine());
    }

    private IEnumerator InkSplatRoutine()
    {
        // Activate & reset alpha
        oilSplat.gameObject.SetActive(true);
        Color color = oilSplat.color;
        color.a = 1f;
        oilSplat.color = color;

        // Stay visible for 2 seconds
        yield return new WaitForSeconds(5f);

        // Fade out over 3 seconds
        float duration = 3f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / duration);

            color.a = alpha;
            oilSplat.color = color;

            yield return null;
        }

        // Ensure fully transparent
        color.a = 0f;
        oilSplat.color = color;

        // Disable object
        oilSplat.gameObject.SetActive(false);
    }
}
