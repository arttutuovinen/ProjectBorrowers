using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPAnimController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>(); 
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("P1Horizontal");
        float vertical = Input.GetAxisRaw("P1Vertical");

        // Check if the player is moving
        bool isMoving = (horizontal != 0 || vertical != 0);

        // Update animator parameter
        animator.SetBool("IsRunning", isMoving);
    }
}
