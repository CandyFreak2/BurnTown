using UnityEngine;

public class StepAnimation : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            animator.enabled = true;
        }
        else
        {
            animator.enabled = false;
        }
    }
}
