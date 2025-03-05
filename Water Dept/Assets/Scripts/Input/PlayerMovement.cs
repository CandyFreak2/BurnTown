using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour
{
    private bool isSpawned = false;

    [Header("Movement")]
    public float MoveSpeed = 5.0f;
    public float AirMoveSpeed = 3.0f;
    public float maxSpeed = 5.0f;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;
    [SerializeField] Transform camTarget;
    [SerializeField] float rotSpeed = 7.0f;
    [Networked] private Vector3 networkedForward { get; set; }

    //movement input
    public Vector3 moveDirection = Vector3.zero;
    [Networked] private NetworkButtons previousButtons { get; set; }

    [Networked] public Interactable carriedObject { get; set; }
    [SerializeField] private TextMeshPro pickupText;

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

    [Header("Audio")]
    [SerializeField] PlayerSFXProfile sfxProfile;
    [Serializable] private class PlayerSFXProfile
    {
        public List<AudioClip> footstep = new List<AudioClip>();
        public List<AudioClip> equipItem = new List<AudioClip>();
        public List<AudioClip> dropItem = new List<AudioClip>();
        public List<AudioClip> stunned = new List<AudioClip>();
        public List<AudioClip> onFire = new List<AudioClip>();
    }

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
            //FreeCameraController.Singleton.SetTarget(camTarget);
            GetComponentInChildren<CineSingleCamera>().SetAsActiveCamera();
        }

        isSpawned = true;
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
            CapSpeed();

            MovePlayer();
            RotatePlayer();
            HandleInteraction();

            previousButtons = action.Buttons;
        }
    }

    private void FixedUpdate()
    {
        if (isSpawned)
        playerObj.forward = Vector3.Slerp(playerObj.forward, networkedForward, Time.fixedDeltaTime * rotSpeed);
    }

    public void RotatePlayer()
    {
        //get camera view direction
        Vector3 camDir = camTarget.forward;
        camDir.y = 0;
        float viewDir = Vector3.Angle(Vector3.forward, camDir);

        //skip rotate if the player has no input
        if (action.LookDelta == Vector2.zero)
        {
            if(moveDirection !=  Vector3.zero)
            {
                orientation.forward = moveDirection;
            }
        }
        else
        {
            //set the orientation to where the player is inputing to look
            orientation.forward = ToVector3(action.LookDelta).normalized;
            orientation.Rotate(0, viewDir, 0);
        }

        Vector3 targetForward = orientation.forward.normalized;

        if (HasStateAuthority)
        {
            networkedForward = targetForward;
        }

        //Rotate the player towards the orientations direction
        playerObj.forward = Vector3.Slerp(playerObj.forward, networkedForward, Time.fixedDeltaTime * rotSpeed);
    }

    public void MovePlayer()
    {
        Quaternion camOffest = Quaternion.Euler(0, camTarget.rotation.eulerAngles.y, 0);
        moveDirection = kcc.TransformRotation * camOffest * ToVector3(action.MoveDirection);

        if (isGrounded)
            kcc.Move(moveDirection * MoveSpeed);
        else
            kcc.Move(action.MoveDirection.normalized * AirMoveSpeed); //Simulate gravity if isn't grounded
    }

    private void HandleInteraction()
    {
        Interactable nearestInteractable = GetNearestInteractable();
        //Pickup hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && !IsHolding() && nearestInteractable != null)
        {
            PickUpObject(nearestInteractable);
            pickupText.text = "";
        }

        //Drop hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Drop) && IsHolding())
        {
            RPC_RequestDrop(Object.Id);
        }

        // Fire hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Shoot) && IsHolding())
        {
            if (carriedObject.interactableType == InteractableType.Flamethrower)
            {
                carriedObject.gameObject.GetComponent<FireHouse>().Spray();
            }
        }

        // Stop firing hose
        if (action.Buttons.WasReleased(previousButtons, InputButton.Shoot) && IsHolding())
        {
            if (carriedObject.interactableType == InteractableType.Flamethrower)
            {
                carriedObject.gameObject.GetComponent<FireHouse>().StartCoroutine(carriedObject.gameObject.GetComponent<FireHouse>().StopSpraying());
            }
        }
    }

    private Interactable GetNearestInteractable()
    {
        Interactable nearestInteractable = null;

        Collider[] interactableObjects = Physics.OverlapSphere(transform.position, 3.5f);
        foreach (Collider collider in interactableObjects)
        {
            // First object in list that is a flamethrower gets picked up ++ Or barrel
            if (collider.gameObject.tag == "FireHoseHead" || collider.gameObject.tag == "Barrel" && !IsHolding())
            {
                nearestInteractable = collider.gameObject.GetComponent<Interactable>();
                pickupText.text = "North Button / E to Pickup";
                return nearestInteractable;
            }
            else
            {
                pickupText.text = "";
            }
        }

        return nearestInteractable;
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

    public void PickUpObject(Interactable _object)
    {
        //Debug.Log(_object.CanBePickedUp());
        if (_object != null && _object.CanBePickedUp())
        {
            RPC_RequestPickUp(_object.Object.Id, Object.Id);
            AudioPlayer.PlaySFX(sfxProfile.equipItem, transform, 0.2f);
        }
    }

    // Asks server very nicely if client can pick up object
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestPickUp(NetworkId _objectId, NetworkId _playerId)
    {
        Interactable interactable = Runner.FindObject(_objectId).GetComponent<Interactable>();
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();

        if (interactable != null && interactable.CanBePickedUp())
        {
            player.carriedObject = interactable;
            player.carriedObject.gameObject.GetComponent<NetworkObject>().AssignInputAuthority(player.Object.InputAuthority);
            player.carriedObject.PickUp(Object.Id);
        }
    }

    // Asks server very nicely if client can drop object
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestDrop(NetworkId _playerId)
    {
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();
        if (player.IsHolding() && player.carriedObject.GetCarrierId() == _playerId)
        {
            player.carriedObject.Drop();
            if (player.carriedObject.interactableType == InteractableType.Flamethrower)
            {
                player.carriedObject.gameObject.GetComponent<FireHouse>().StartCoroutine(carriedObject.gameObject.GetComponent<FireHouse>().StopSpraying());
            }
            player.carriedObject.gameObject.GetComponent<NetworkObject>().RemoveInputAuthority();
            player.carriedObject = null;
        }
    }

    public bool IsHolding()
    {
        return carriedObject != null;
    }
}
