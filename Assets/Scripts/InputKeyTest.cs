// InputKeyTest.cs
using UnityEngine;

public class InputKeyTest : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
            Debug.Log("[InputKeyTest] anyKeyDown at frame " + Time.frameCount);

        if (Input.GetKeyDown(KeyCode.L))
            Debug.Log("[InputKeyTest] L key pressed (Input.GetKeyDown)");

        if (Input.GetKeyDown(KeyCode.BackQuote))
            Debug.Log("[InputKeyTest] BackQuote pressed (Input.GetKeyDown)");
    }
}
