using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SPCheckInJar : MonoBehaviour
{
    public TextMeshProUGUI revealText;
    public string playerTag = "SmallPlayer"; 
    public float resetDelay = 5f;

    private void Start()
    {   
            revealText.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            revealText.gameObject.SetActive(true);
            StartCoroutine(RestartSceneAfterDelay());
        }
    }

    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay); // Wait for the specified delay
        Scene currentScene = SceneManager.GetActiveScene(); // Get the current active scene
        SceneManager.LoadScene(currentScene.name); // Reload the scene
    }
    
}
