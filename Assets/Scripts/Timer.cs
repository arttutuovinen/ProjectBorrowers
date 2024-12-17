using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bpWins;
    public float remainingTime;
    public float resetDelay = 5f;

    void Start()
    {
        bpWins.gameObject.SetActive(false);   
    }
    void Update()
    {
        if(remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else if(remainingTime < 0)
        {
            Debug.Log("bp wins");
            remainingTime = 0;
            bpWins.gameObject.SetActive(true);
            bpWins.text = "BIG PLAYER WINS";
            StartCoroutine(RestartSceneAfterDelay());
        }
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay); // Wait for the specified delay
        Scene currentScene = SceneManager.GetActiveScene(); // Get the current active scene
        SceneManager.LoadScene(currentScene.name); // Reload the scene
    }
}
