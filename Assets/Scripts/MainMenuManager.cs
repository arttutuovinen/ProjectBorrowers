using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load for joining (replace with your join scene name)")]
    public string GameScene = "GameScene";

    [Tooltip("CanvasGroup on the main menu panel")]
    public CanvasGroup canvasGroup;

    [Tooltip("How long the fade takes (seconds)")]
    public float fadeDuration = 0.5f;

    bool isFading = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    // Called by Start Game button
    public void StartGame()
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad());
    }

    private System.Collections.IEnumerator FadeOutAndLoad()
    {
        isFading = true;

        float t = 0f;
        float startAlpha = canvasGroup.alpha;

        // Fade from current alpha to 0
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }

        // Ensure fully invisible
        canvasGroup.alpha = 0f;

        // Load next scene
        SceneManager.LoadScene(GameScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
