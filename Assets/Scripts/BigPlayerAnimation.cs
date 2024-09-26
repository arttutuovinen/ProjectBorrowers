using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlayerAnimation : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        // Get the Animator component attached to the character
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the "P2Grab" button is pressed
        if (Input.GetButtonDown("P2PickUp"))
        {
            // Trigger the "Grab" animation
            animator.SetBool("isGrabbing", true);
            Debug.Log("Animation works");
        }
    }
}
