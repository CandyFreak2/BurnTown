using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5.0f;
    public float AirMoveSpeed = 3.0f;
    public float maxSpeed = 5.0f;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    [SerializeField] Transform camTarget;
    [SerializeField] float rotSpeed = 7.0f;

    //movement input
    public Vector3 moveDirection = Vector3.zero;
    
    PlayerInputHandeler input;
    PlayerInputAction action;
    [SerializeField] Rigidbody rb;
    [SerializeField] SimpleKCC kcc;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] bool isGrounded;

    [Header("Rb properties")]
    [SerializeField] float Mass = 1.0f;
    [SerializeField] float Drag = 5.0f;
    //[SerializeField] float AirDrag = 7.0f;

    private void Start()
    {
        input = GetComponent<PlayerInputHandeler>();
        //input.onInputSignal += RecieveInput;
        rb.mass = Mass;
    }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y);

        if (HasInputAuthority)
        {
            FreeCameraController.Singleton.SetTarget(camTarget);
        }
    }

    //overridden by FixedUpdateNetwork
    void RecieveInput(PlayerInputAction action)
    {
        this.action = action;
        CheckGround();
        RotatePlayer();
        CapSpeed();
    }

    private void FixedUpdate()
    {
        if (GameManager.TimeScale != 0)
        {
            //MovePlayer();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // gets input from server
        if (GetInput(out PlayerInputAction input))
        {
            action = input;

            CheckGround();
            RotatePlayer();
            CapSpeed();

            MovePlayer();

        }
    }

    public void RotatePlayer()
    {
        //skip rotate if the player has no input
        if (action.LookDelta == Vector2.zero)
            return;

        //get camera view direction
        Vector3 camDir = camTarget.forward;
        camDir.y = 0;
        float viewDir = Vector3.Angle(Vector3.forward, camDir);

        //set the orientation to where the player is inputing to look
        orientation.forward = ToVector3(action.LookDelta).normalized;
        orientation.Rotate(0, viewDir, 0);

        //Rotate the player towards the orientations direction
        playerObj.forward = Vector3.Lerp(playerObj.forward, orientation.forward.normalized, Time.deltaTime * rotSpeed);
    }

    public void MovePlayer()
    {
        moveDirection = kcc.TransformRotation * new Vector3(action.MoveDirection.x, 0.0f, action.MoveDirection.y);

        if (isGrounded)
            kcc.Move(moveDirection * MoveSpeed);
        else
            kcc.Move(action.MoveDirection.normalized * AirMoveSpeed); //Simulate gravity if isn't grounded
    }

    public bool CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.7f, groundMask);

        if (isGrounded)
            rb.linearDamping = Drag;
        else
            rb.linearDamping = 0;

        return isGrounded;
    }

    public void CapSpeed()
    {
        Vector3 currentVelcity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (currentVelcity.magnitude > maxSpeed)
        {
            Vector3 maxVel = currentVelcity.normalized * maxSpeed;
            maxVel.y = rb.linearVelocity.y;
            rb.linearVelocity = maxVel;
        }
    }

    Vector3 ToVector3(Vector2 input)
    {
        return new Vector3(input.x, 0, input.y);
    }
}
