using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScene : MonoBehaviour
{
    void Update()
    {
        // Detect R2 button (mapped as "Fire2" in Unity's default Input)
        if (Input.GetButtonDown("Restart"))
        {
            SceneRestart();
        }
    }

    void SceneRestart()
    {
        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
