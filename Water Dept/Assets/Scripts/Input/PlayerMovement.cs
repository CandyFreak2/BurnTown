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
    private GameObject fireHoseParent;
    private GameObject fireHoseHead;
    [Networked] public NetworkBool isHolding { get; set; } //public for pickup collider
    [Networked] private NetworkId heldHoseId {  get; set; }

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
        //Drop hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && isHolding)
        {
            if (/*HasInputAuthority || HasStateAuthority &&*/ fireHoseParent != null)
            {
                DropHose();
            }
        }

        // Fire hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Shoot) && isHolding)
        {
            Debug.Log("On");
            if (fireHoseHead != null)
            {
                fireHoseHead.GetComponent<FireHouse>().Spray();
                
            }
        }

        // Stop firing hose
        if (action.Buttons.WasReleased(previousButtons, InputButton.Shoot) && isHolding)
        {
            Debug.Log("Off");
            if (fireHoseHead != null)
            {
                fireHoseHead.GetComponent<FireHouse>().StartCoroutine(fireHoseHead.GetComponent<FireHouse>().StopSpraying());
                
            }
        }

        //// Update position of hose if hose is held and has input or state authority
        //if (isHolding && fireHoseParent != null && (HasStateAuthority || HasInputAuthority))
        //{
        //    //RPC_SetHosePosition(hand.position, hand.rotation);
        //}
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
        // Update local data
        fireHoseParent = _hoseParent.gameObject;
        fireHoseHead = _hoseHead.gameObject;

        isHolding = true;
        fireHoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        fireHoseHead.GetComponent<Rigidbody>().isKinematic = true;

        // Update server data
        RPC_RequestPickUpHose(Object.Id, _hoseParent.GetComponent<NetworkObject>().Id);
    }

    private void DropHose()
    {
        RPC_RequestDropHose(Object.Id, fireHoseParent.GetComponent<NetworkObject>().Id);

        if (isHolding) //Update local data. This works somehow?
        {
            isHolding = false;
            fireHoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            fireHoseHead.GetComponent<Rigidbody>().isKinematic = false;
            fireHoseHead.GetComponent<FireHouse>().DropHose();

            // Clear references
            fireHoseParent.transform.SetParent(null);
            fireHoseParent = null;
            fireHoseHead = null;
            heldHoseId = default;
        }
    }

    // Asks server very nicely if client can pick up hose
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestPickUpHose(NetworkId _playerId, NetworkId _hoseId)
    {
        // Player that is picking up hose and hose objects
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();
        GameObject hoseParent = Runner.FindObject(_hoseId).gameObject;

        // Give player that is picking up hose input authority over hose
        hoseParent.GetComponent<NetworkObject>().AssignInputAuthority(player.Object.InputAuthority);

        // Pick up hose
        player.isHolding = true;
        player.heldHoseId = _hoseId;
        player.fireHoseParent = hoseParent;
        player.fireHoseHead = hoseParent.transform.GetChild(0).gameObject;

        player.fireHoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        player.fireHoseHead.GetComponent<Rigidbody>().isKinematic = true;

        RPC_SetHosePosition(player.hand.position, player.hand.rotation, _playerId);
    }

    // Sync position with all clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetHosePosition(Vector3 _position, Quaternion _rotation, NetworkId _playerId)
    {
        NetworkObject hoseParent = Runner.FindObject(heldHoseId);
        if (hoseParent != null)
        {
            hoseParent.transform.position = _position;
            hoseParent.transform.rotation = _rotation;
            hoseParent.transform.SetParent(hand);

            hoseParent.transform.GetChild(0).transform.localPosition = Vector3.zero;
            hoseParent.transform.GetChild(0).transform.localRotation = Quaternion.identity;
        }
    }

    // Asks server very nicely if client can drop hose
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDropHose(NetworkId _playerId, NetworkId _hoseId)
    {
        if (!isHolding || fireHoseParent == null)
            return;

        //Player that is dropping hose and hose objects
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();
        GameObject hoseParent = Runner.FindObject(_hoseId).gameObject;
        GameObject hoseHead = hoseParent.transform.GetChild(0).gameObject;

        // Remove input authority
        hoseParent.GetComponent<NetworkObject>().RemoveInputAuthority();

        // Drop hose
        player.isHolding = false;
        hoseParent.transform.SetParent(null);
        hoseHead.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        hoseHead.GetComponent<Rigidbody>().isKinematic = false;
        hoseHead.GetComponent<FireHouse>().DropHose();

        RPC_DropHose(hoseParent.transform.position, hoseParent.transform.rotation);
    }

    // Sync position with all clients after dropping
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_DropHose(Vector3 _position, Quaternion _rotation)
    {
        NetworkObject hoseParent = Runner.FindObject(heldHoseId);

        if (hoseParent != null)
        {
            hoseParent.transform.position = _position;
            hoseParent.transform.rotation = _rotation;
            hoseParent.transform.SetParent(null);
        }
    }

}
