using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SPCheckInJar : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public string playerTag = "SmallPlayer"; 
    public float resetDelay = 3f;
    public GameObject smallPlayer;
    public GameObject finish;
    private int score = 0;

    private void Start()
    {
        scoreText.text = score.ToString();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Add 1 point to the score
            score++;
            scoreText.text = score.ToString();
            StartCoroutine(TeleportAfterDelay());
        }
    }

    private IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        CharacterController controller = smallPlayer.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false; // disable before teleporting
            smallPlayer.transform.position = finish.transform.position;
            controller.enabled = true;  // re-enable
        }
        else
        {
            // fallback if no CharacterController found
            smallPlayer.transform.position = finish.transform.position;
        }
    }
    
}
