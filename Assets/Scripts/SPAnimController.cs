using System.Collections;
using UnityEngine;

public class SPAnimController : MonoBehaviour
{

    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private float movementThreshold = 0.1f;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateRunParameter();
    }

    private void UpdateRunParameter()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isRunning = Mathf.Abs(h) > movementThreshold || Mathf.Abs(v) > movementThreshold;
        animator.SetBool(isRunningParam, isRunning);
    }

    
}

