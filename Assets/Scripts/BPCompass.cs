using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BPCompass : MonoBehaviour
{
    public GameObject smallPlayer;
    public GameObject bpCompass;
    

    void Start()
    {
        bpCompass.SetActive(false);
    }

    public void UseCompass()
    {
        bpCompass.SetActive(true);
        StartCoroutine(UpdateCompass());
        Debug.Log("Kompassi toimii");
    }
    private IEnumerator UpdateCompass()
{
    float duration = 6f; // Example duration in seconds
    float elapsed = 0f;

    while (elapsed < duration)
    {
        Vector3 directionToSmallPlayer = smallPlayer.transform.position - transform.position;
        directionToSmallPlayer.y = 0;

        if (directionToSmallPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToSmallPlayer);
            transform.rotation = targetRotation;
        }

        elapsed += Time.deltaTime;
        yield return null; // Wait for the next frame
    }

    bpCompass.SetActive(false); // Deactivate compass after duration
}
}
