// PauseMenuController.cs (with fade)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PauseMenuController : MonoBehaviourPunCallbacks
{
    [Tooltip("Assign the Pause Menu panel GameObject (can be initially inactive).")]
    public GameObject pauseMenuPanel;

    [Tooltip("Optional: which UI element to select when opening (drag the Continue button here).")]
    public GameObject defaultSelected;

    [Tooltip("If true, we will set Time.timeScale = 0 when menu opens, and restore when closed.")]
    public bool pauseTime = false;

    [Tooltip("Scene name to load when quitting to the main menu")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("CanvasGroup used for full-screen fade. Optional but recommended.")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("Fade duration in seconds for the transition")]
    public float fadeDuration = 0.6f;

    // Only allow toggling when the local player is in a Photon room
    bool canToggle = false;

    bool isOpen = false;

    // Flag to prevent multiple disconnect attempts
    bool isDisconnecting = false;

    void Start()
    {
        if (pauseMenuPanel == null)
            Debug.LogWarning("[PauseMenuController] pauseMenuPanel not assigned. Assign it in the inspector.");

        // Ensure the panel is hidden at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Determine initial toggle permission
        canToggle = PhotonNetwork.InRoom;

        // Ensure fade canvas group initial state
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false; // allow clicks unless fading
            fadeCanvasGroup.gameObject.SetActive(true); // keep active so coroutine can run
        }
    }

    void Update()
    {
        // Toggle on Escape press ONLY if allowed (i.e. in room)
        if (canToggle && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        canToggle = true;
        Debug.Log("[PauseMenuController] OnJoinedRoom: pause menu enabled.");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        // disable toggling if we leave the room (back to lobby)
        canToggle = false;

        // ensure menu is closed when leaving
        if (isOpen)
        {
            CloseMenuImmediate();
        }

        Debug.Log("[PauseMenuController] OnLeftRoom: pause menu disabled and closed.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        Debug.Log("[PauseMenuController] OnDisconnected: " + cause.ToString());

        // If we were disconnecting as part of QuitToMainMenu, load the main menu now
        if (isDisconnecting)
        {
            isDisconnecting = false;

            // Make sure timeScale and cursor are sensible
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.LogError("[PauseMenuController] mainMenuSceneName is empty - cannot load scene.");
            }
        }
    }

    public void ToggleMenu()
    {
        if (pauseMenuPanel == null) return;

        isOpen = !isOpen;
        pauseMenuPanel.SetActive(isOpen);

        if (isOpen) OpenMenu();
        else CloseMenu();
    }

    void OpenMenu()
    {
        // show cursor, unlock it so player can click UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // optionally pause the game (be careful in multiplayer)
        if (pauseTime) Time.timeScale = 0f;

        // Set selected UI element for keyboard/controller navigation
        if (defaultSelected != null)
        {
            EventSystem es = EventSystem.current;
            if (es != null)
            {
                es.SetSelectedGameObject(null);
                es.SetSelectedGameObject(defaultSelected);
            }
        }
    }

    void CloseMenu()
    {
        // hide/lock cursor again for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // resume time if we paused
        if (pauseTime) Time.timeScale = 1f;

        // clear selected object in EventSystem
        EventSystem es = EventSystem.current;
        if (es != null)
            es.SetSelectedGameObject(null);
    }

    // Called when leaving room or when we must force close immediately without toggling flow
    void CloseMenuImmediate()
    {
        isOpen = false;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Restore cursor to a sensible state for lobby (unlocked/visible)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseTime) Time.timeScale = 1f;
    }

    // Convenience public method you can hook to Continue button OnClick
    public void Continue()
    {
        if (isOpen)
            ToggleMenu(); // will close
    }

    // ========== Quit to Main Menu (with fade) ==========
    public void QuitToMainMenu()
    {
        if (isDisconnecting) return;

        // If not connected to Photon, just fade and load the main menu
        if (!PhotonNetwork.IsConnected)
        {
            StartCoroutine(FadeThenLoadSceneDirect());
            return;
        }

        // Connected: fade, then disconnect; OnDisconnected will load main menu
        isDisconnecting = true;

        // Close the pause menu UI immediately
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Ensure timeScale is normal during transition
        Time.timeScale = 1f;

        // Make cursor visible during transition
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Start fade, then disconnect
        StartCoroutine(FadeThenDisconnect());
    }

    IEnumerator FadeThenDisconnect()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        Debug.Log("[PauseMenuController] Fade complete — disconnecting from Photon...");

        // Blocks raycasts while disconnecting
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;

        PhotonNetwork.Disconnect();
    }

    IEnumerator FadeThenLoadSceneDirect()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Direct load (no Photon)
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError("[PauseMenuController] mainMenuSceneName is empty - cannot load scene.");
    }

    // Generic fade coroutine (alpha from a->b)
    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            // No fade canvas assigned: quick wait to avoid abruptness
            yield return new WaitForSeconds(duration);
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;
        float t = 0f;
        fadeCanvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // use unscaled so it still works if timeScale==0
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
    }
}
