using UnityEngine;

public class TPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5.0f;
    public float AirMoveSpeed = 3.0f;
    public float maxSpeed = 5.0f;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    [SerializeField] Transform camTarget;
    [SerializeField] float rotSpeed = 7.0f;

    //movement
    public Vector3 moveDirection = Vector3.zero;
    
    TPlayerInputHandeler input;
    TPlayerInputAction action;
    Rigidbody rb;
    CharacterController charCtrl;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] bool isGrounded;

    [Header("Rb properties")]
    [SerializeField] float Mass = 1.0f;
    [SerializeField] float Drag = 5.0f;
    //[SerializeField] float AirDrag = 7.0f;

    private void Awake() {
        camTarget = Camera.main.transform;
    }

    private void Start()
    {
        input = GetComponent<TPlayerInputHandeler>();
        input.onInputSignal += RecieveInput;

        rb = GetComponent<Rigidbody>();
        charCtrl = GetComponent<CharacterController>();

        rb.mass = Mass;
    }

    void RecieveInput(TPlayerInputAction action)
    {
        this.action = action;
        CheckGround();
        RotatePlayer();
        CapSpeed();
    }

    /*private void Update()
    {
        if (GameManager.TimeScale != 0)
        {

            
        }
    }*/

    private void FixedUpdate()
    {
        if (GameManager.TimeScale != 0)
        {
            RotatePlayer();
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
        moveDirection.x = action.MoveDirection.x * MoveSpeed;
        moveDirection.z = action.MoveDirection.y * MoveSpeed;

        if (isGrounded)
        {
            rb.AddForce(moveDirection * MoveSpeed * 10f * Time.fixedDeltaTime, ForceMode.Impulse);
            //charCtrl.Move(moveDirection * MoveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            //rb.AddForce(moveDirection.normalized * AirMoveSpeed * 10f, ForceMode.Force);
        }
    }

    public bool CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.7f, groundMask);

        if (isGrounded)
        {
            rb.linearDamping = Drag;
        }
        else
        {
            rb.linearDamping = 0;
        }

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
