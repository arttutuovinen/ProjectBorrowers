using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Player Scores")]
    public TextMeshProUGUI smallPlayerScoreText; // Drag SmallPlayer's score text here
    public TextMeshProUGUI bigPlayerScoreText;   // Drag BigPlayer's score text here

    [Header("Win Messages")]
    public TextMeshProUGUI smallPlayerWinsText;  // Drag SmallPlayer wins text here
    public TextMeshProUGUI bigPlayerWinsText;    // Drag BigPlayer wins text here

    [Header("Win Condition")]
    public int winningScore = 5;
    public float restartDelay = 3f;

    private bool gameEnded = false;

    private void Start()
    {
        smallPlayerWinsText.gameObject.SetActive(false);
        bigPlayerWinsText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameEnded) return;

        int smallScore = GetScoreFromText(smallPlayerScoreText);
        int bigScore = GetScoreFromText(bigPlayerScoreText);

        if (smallScore >= winningScore)
        {
            if (smallPlayerWinsText != null) smallPlayerWinsText.gameObject.SetActive(true);
            EndGame();
        }

        if (bigScore >= winningScore)
        {
            if (bigPlayerWinsText != null) bigPlayerWinsText.gameObject.SetActive(true);
            EndGame();
        }
    }

    private int GetScoreFromText(TextMeshProUGUI scoreText)
    {
        if (scoreText == null) return 0;

        int score;
        if (int.TryParse(scoreText.text, out score))
            return score;

        return 0;
    }

    private void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        StartCoroutine(RestartSceneAfterDelay());
    }

    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
