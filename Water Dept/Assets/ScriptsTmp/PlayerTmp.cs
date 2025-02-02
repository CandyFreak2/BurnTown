using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class PlayerTmp : NetworkBehaviour
{
    //[SerializeField] private NetworkCharacterController _cc;

    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float jumpImpulse = 10.0f;

    private Vector3 moveDirection = Vector3.zero;
    private float jump = 0.0f;

    [Networked] private NetworkButtons previousButtons { get; set; }
    
    [SerializeField]
    Ball ballPrefab;
    [Networked] private TickTimer ballDelay { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y);

        if (HasInputAuthority)
        {
            FreeCameraController.Singleton.SetTarget(camTarget);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            input.direction.Normalize();

            // Change kcc rotation based on input and change localRotation of target to move based on x component of kcc rotation
            kcc.AddLookRotation(input.lookDelta * lookSensitivity);
            UpdateCamTarget();

            moveDirection = kcc.TransformRotation * new Vector3(input.direction.x, 0.0f, input.direction.y);
            jump = 0.0f;

            if (input.buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.IsGrounded)
            {
                jump = jumpImpulse;
            }

            kcc.Move(moveDirection.normalized * speed, jump);
            previousButtons = input.buttons;

            // Update rotation
            //_cc.transform.Rotate(Vector3.up, data.horizontalRotation, Space.World);
            //_cc.transform.Rotate(Vector3.right, data.verticalRotation, Space.Self);

            if (HasStateAuthority && ballDelay.ExpiredOrNotRunning(Runner))
            {
                if (input.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    ballDelay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(ballPrefab, kcc.transform.position + kcc.transform.forward + kcc.transform.up, Quaternion.LookRotation(kcc.transform.forward), Object.InputAuthority, (runner, o) =>
                    {
                        o.GetComponent<Ball>().Init();
                    });
                }
            }
        }
    }

    //Override Render function so that camera movement is smoothed on clientside and isn't getting updated per server tick
    public override void Render()
    {
        UpdateCamTarget();
    }

    public void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0.0f, 0.0f);
    }
}
