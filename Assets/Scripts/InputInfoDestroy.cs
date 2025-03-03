using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputInfoDestroy : MonoBehaviour
{
    public Image SPInput;
    public Image BPInput;
    void Update()
    {
        if (Input.GetButtonDown("P1Jump"))
        {
            SPInput.gameObject.SetActive(false);
        }
        if (Input.GetButtonDown("P2Crouch"))
        {
            BPInput.gameObject.SetActive(false);
        }
    }
}
