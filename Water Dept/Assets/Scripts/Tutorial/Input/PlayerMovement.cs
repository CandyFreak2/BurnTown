using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5.0f;
    public float AirMoveSpeed = 3.0f;
    public float maxSpeed = 5.0f;
    [SerializeField] Transform mainCamera;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    [SerializeField] float rotSpeed = 7.0f;

    //rotation
    Vector3[] deltaArray = new Vector3[25];
    Vector3 deltaAvg;
    int deltaIndex = 0;

    //movement
    public Vector3 moveDirection = Vector3.zero;
    
    PlayerInputHandeler input;
    PlayerInputAction action;
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

    private void Awake()
    {
        for(int i = 0; i < deltaArray.Length; i++)
            deltaArray[i] = Vector3.zero;

        mainCamera = Camera.main.transform;
    }

    private void Start()
    {
        input = GetComponent<PlayerInputHandeler>();
        input.onInputSignal += RecieveInput;

        rb = GetComponent<Rigidbody>();
        charCtrl = GetComponent<CharacterController>();

        rb.mass = Mass;
    }

    void RecieveInput(PlayerInputAction action)
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
            AverageDelta();
            MovePlayer();
        }
    }

    void AverageDelta()
    {
        deltaArray[deltaIndex].x = action.LookDelta.x;
        deltaArray[deltaIndex].z = action.LookDelta.y;
        deltaIndex++;

        if(deltaIndex >= deltaArray.Length)
            deltaIndex = 0;

        //average out all the delta pointers over 25 fixed frames
        foreach(Vector3 delta in deltaArray)
            deltaAvg += delta;
        deltaAvg /= deltaArray.Length;
    }

    public void RotatePlayer()
    {
        Vector3 camDir = mainCamera.forward;
        camDir.y = 0;
        float viewDir = Vector3.Angle(Vector3.forward, camDir);
        orientation.forward = deltaAvg.normalized;
        orientation.Rotate(0, viewDir, 0);

        if (action.LookDelta != Vector2.zero)
        {
            playerObj.forward = Vector3.Lerp(playerObj.forward, orientation.forward.normalized, Time.deltaTime * rotSpeed);
        }
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

}
