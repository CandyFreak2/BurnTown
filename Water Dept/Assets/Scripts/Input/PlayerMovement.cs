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
    [Networked] private NetworkButtons previousButtons { get; set; }

    // Hose data
    public Transform hand;
    private GameObject fireHoseParent, fireHoseHead;
    [Networked] public bool isHolding { get; set; } //public for pickup collider

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

        isHolding = false;
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
            HandleInteraction();

            //if (fireHoseParent != null)
            //{
            //    fireHoseParent.transform.position = hand.position;
            //    fireHoseParent.transform.rotation = hand.rotation;

            //    fireHoseHead.transform.localPosition = Vector3.zero;
            //    fireHoseHead.transform.localRotation = Quaternion.identity;
            //}

            previousButtons = action.Buttons;
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

    private void HandleInteraction()
    {
        if (!HasInputAuthority)
            return;

        //Drop hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && isHolding)
        {
            Debug.Log("Dropped Hose");
            if (fireHoseParent != null)
            {
                fireHoseParent.transform.SetParent(null, true);
                fireHoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                fireHoseHead.GetComponent<Rigidbody>().isKinematic = false;
                fireHoseHead.GetComponent<FireHouse>().DropHose();

                fireHoseParent = null;
                fireHoseHead = null;
                isHolding = false;
            }
        }

        // Fire hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Shoot) && isHolding)
        {
            if (fireHoseHead != null)
            {
                fireHoseHead.GetComponent<FireHouse>().Spray();
            }
        }

        // Stop firing hose
        if (action.Buttons.WasReleased(previousButtons, InputButton.Shoot) && isHolding)
        {
            if (fireHoseHead != null)
            {
                fireHoseHead.GetComponent<FireHouse>().StartCoroutine(fireHoseHead.GetComponent<FireHouse>().StopSpraying());
            }
        }
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

    public void PickUpHose(GameObject _hoseParent, GameObject _hoseHead)
    {
        if (!HasInputAuthority)
            return;

        // Request state authority
        NetworkObject hoseNetworkObject = _hoseParent.GetComponent<NetworkObject>();
        if (hoseNetworkObject != null )
        {
            hoseNetworkObject.RequestStateAuthority(); //should work cus AllowStateAuthorityOverride is true on hose
        }

        fireHoseParent = _hoseParent.gameObject;
        fireHoseHead = _hoseHead.gameObject;

        Debug.Log("Grabbed Hose");
        isHolding = true;
        fireHoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        fireHoseHead.GetComponent<Rigidbody>().isKinematic = true;

        RPC_SetHosePosition(hand.transform.position, hand.transform.rotation);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetHosePosition(Vector3 _position, Quaternion _rotation)
    {
        if (fireHoseParent != null)
        {
            fireHoseParent.transform.position = _position;
            fireHoseParent.transform.rotation = _rotation;
            fireHoseParent.transform.SetParent(hand);
        }
    }
}
