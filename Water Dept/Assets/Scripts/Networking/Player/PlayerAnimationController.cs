using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _playerAnimator;
    private SimpleKCC _playerKCC;
    private float playerSpeed;

    private void Awake()
    {
        _playerAnimator = GetComponentInParent<Animator>();
        _playerKCC = GetComponentInParent<SimpleKCC>();
    }

    private void Update()
    {
        playerSpeed = _playerKCC.RealSpeed;
        _playerAnimator.SetFloat("Speed", playerSpeed);
    }
}
