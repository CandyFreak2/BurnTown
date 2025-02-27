using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _playerAnimator;
    private Rigidbody _playerRB;
    private float playerSpeed;

    private void Awake()
    {
        _playerAnimator = GetComponentInParent<Animator>();
        _playerRB = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        playerSpeed = _playerRB.linearVelocity.magnitude;
        _playerAnimator.SetFloat("Speed", playerSpeed);
    }
}
