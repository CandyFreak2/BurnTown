using Fusion;
using UnityEngine;

public class PlayerTmp : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterController _cc;

    private Vector3 moveDirection = Vector3.zero;

    [SerializeField]
    Ball ballPrefab;
    [Networked] private TickTimer ballDelay { get; set; }

    private void Awake()
    {
        //_cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            //_cc.Move(5 * data.direction * Runner.DeltaTime);
            //_cc.transform.position += data.direction * 5 * Runner.DeltaTime;

            // Update pos
            moveDirection = _cc.transform.forward * data.direction.z + _cc.transform.right * data.direction.x;
            _cc.transform.position += moveDirection * 5 * Runner.DeltaTime;

            // Update rotation
            _cc.transform.Rotate(Vector3.up, data.horizontalRotation, Space.World);
            _cc.transform.Rotate(Vector3.right, data.verticalRotation, Space.Self);

            if (HasStateAuthority && ballDelay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    ballDelay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(ballPrefab, _cc.transform.position + _cc.transform.forward, Quaternion.LookRotation(_cc.transform.forward), Object.InputAuthority, (runner, o) =>
                    {
                        o.GetComponent<Ball>().Init();
                    });
                }
            }
        }
    }
}
