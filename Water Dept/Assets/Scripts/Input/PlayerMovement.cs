using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using Unity.Cinemachine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using System.Threading;

public class PlayerMovement : NetworkBehaviour
{
    private bool isSpawned = false;
    private bool canEnter = true;

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
    private Vector3 moveDirection = Vector3.zero;
    [Networked] private NetworkButtons previousButtons { get; set; }

    [Networked] public Interactable carriedObject { get; set; }
    [SerializeField] private TextMeshPro pickupText;
    [SerializeField] private CarController truckController;

    [SerializeField] private GameObject indicator;

    PlayerInputHandeler input;
    PlayerInputAction action;
    [SerializeField] Rigidbody rb;
    [SerializeField] SimpleKCC kcc;

    //Barrel interactions
    BarrelController barrelController;

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
        barrelController = GetComponent<BarrelController>();
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
            //ObjectiveIndicatorList.Singleton.SetIndicator(indicator);
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

        //forces
        if (boosh)
        {
            SimulateExplosion();
        }
    }

    private void FixedUpdate()
    {
        if (isSpawned)
        playerObj.forward = Vector3.Slerp(playerObj.forward, networkedForward, Time.fixedDeltaTime * rotSpeed);

        barrelController.SetOrientation(playerObj.forward);

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
        GameObject nearestInteractable = GetNearestInteractable();

        //Pickup hose // barrel
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && !IsHolding() && nearestInteractable != null && CanPickup(nearestInteractable))
        {
            PickUpObject(nearestInteractable.GetComponent<Interactable>());
            pickupText.text = "";

            //if barrel is picked up, queue schematic
            if (nearestInteractable.tag == "Barrel")
            {
                AudioPlayer.PlaySFX(nearestInteractable.gameObject.GetComponent<BarrelInfo>().pickupSFX, playerObj.position, 0.1f);
                
                barrelController.RPC_ToggleBarrel(Object.Id);
            }
        }

        // Get in truck
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && nearestInteractable != null && nearestInteractable.gameObject.tag == "FireTruck" && canEnter)
        {
            GetInTruck(nearestInteractable);
            pickupText.text = "";
        }

        //Drop hose
        if (action.Buttons.WasPressed(previousButtons, InputButton.Drop) && IsHolding())
        {
            if(carriedObject.interactableType == InteractableType.Barrel)
            {
                barrelController.RPC_PlaceBarrel(Object.Id);
            }
            RPC_RequestDrop(Object.Id);
        }

        // Fire hose // throw barrel
        if (action.Buttons.WasPressed(previousButtons, InputButton.Shoot) && IsHolding())
        {
            if (carriedObject.interactableType == InteractableType.Flamethrower)
            {
                carriedObject.gameObject.GetComponent<Flamethrower>().Spray();
            }
            else if (carriedObject.interactableType == InteractableType.Barrel)
            {
                RPC_RequestDrop(Object.Id);
                barrelController.RPC_ThrowBarrel(Object.Id);
            }
        }

        // Stop firing hose
        if (action.Buttons.WasReleased(previousButtons, InputButton.Shoot) && IsHolding())
        {
            if (carriedObject.interactableType == InteractableType.Flamethrower)
            {
                carriedObject.gameObject.GetComponent<Flamethrower>().StopSpraying();
            }
        }

        //if holding barrel and throw, throw barrel
        if (action.Buttons.WasPressed(previousButtons, InputButton.Throw) && !IsHolding() && nearestInteractable != null)
        {
            //if barrel is picked up, queue schematic
            if (nearestInteractable.tag == "Barrel")
            {
                //barrelController.BarrelSchematics(playerObj);
            }

        }
    }

    private GameObject GetNearestInteractable()
    {
        GameObject nearestInteractable = null;

        if (HasInputAuthority && !IsHolding())
        {
            Collider[] interactableObjects = Physics.OverlapSphere(kcc.transform.position, 3.5f);
            foreach (Collider collider in interactableObjects)
            {
                // First object in list that is a flamethrower gets picked up ++ Or barrel
                if (collider.gameObject.tag == "FireTruck")
                {
                    nearestInteractable = collider.gameObject;
                    pickupText.text = "North Button / E to Enter Fire Truck";
                    return nearestInteractable;
                }
                else if (collider.gameObject.tag == "FireHoseHead")
                {
                    nearestInteractable = collider.gameObject;
                    pickupText.text = "North Button / E to Pickup Flamethrower";
                    return nearestInteractable;
                }
                else if (collider.gameObject.tag == "Barrel")
                {
                    nearestInteractable = collider.gameObject;
                    pickupText.text = "North Button / E to Pickup Barrel";
                    return nearestInteractable;
                }
                else
                {
                    pickupText.text = "";
                }
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

    public void DeactivateSystems()
    {
        GetComponent<SimpleKCC>().enabled = false;

        foreach (Transform child in gameObject.transform) //crazy tech bro
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ReactivateSystems()
    {
        GetComponent<SimpleKCC>().enabled = true;

        foreach (Transform child in gameObject.transform) //crazy tech bro
        {
            child.gameObject.SetActive(true);
        }
    }

    private void GetInTruck(GameObject _truck)
    {
        //truckController = _truck.GetComponent<CarController>();

        truckController.enabled = true;

        if (truckController != null)
        {
            // set data
            truckController.SetTruckParent(_truck);
            truckController.SetCarRB(_truck.GetComponent<Rigidbody>());
            truckController.SetFrontTires(_truck.transform.Find("FL Tire").gameObject.GetComponent<Tire>());
            truckController.SetFrontTires(_truck.transform.Find("FR Tire").gameObject.GetComponent<Tire>());
            truckController.SetBackTires(_truck.transform.Find("BL Tire").gameObject.GetComponent<Tire>());
            truckController.SetBackTires(_truck.transform.Find("BR Tire").gameObject.GetComponent<Tire>());
            truckController.SetTireAngles();
            truckController.SetDriverPos(_truck.transform.Find("DriverPos").gameObject.transform);

            // lower player camera priority
            if (HasInputAuthority)
            {
                CinemachineCamera camera = GetComponentInChildren<CinemachineCamera>();
                camera.Priority = -1;
            }

            // change player input to car input
            truckController.PlayerEnterTruck(this);
        }

        RPC_RequestGetInTruck(_truck.GetComponent<NetworkObject>().Id, Object.Id);
    }

    public void GetOutTruck()
    {
        truckController.enabled = false;

        // raise player camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = GetComponentInChildren<CinemachineCamera>();
            camera.Priority = 1;
        }

        // Add delay to player being able to enter so player doesn't instantly leave off of exit input
        canEnter = false;
        Invoke("EnterDelay", 0.5f);

        RPC_RequestGetOutTruck(Object.Id);
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

    // Asks server very nicely if client can get in whip
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestGetInTruck(NetworkId _truckId, NetworkId _playerId)
    {
        GameObject truck = Runner.FindObject(_truckId).gameObject;
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();

        if (truck != null)
        {
            CarController playerTruckController = player.truckController;
            playerTruckController.enabled = true;

            if (playerTruckController != null)
            {
                // set data
                playerTruckController.SetTruckParent(truck);
                playerTruckController.SetCarRB(truck.GetComponent<Rigidbody>());
                playerTruckController.SetFrontTires(truck.transform.Find("FL Tire").gameObject.GetComponent<Tire>());
                playerTruckController.SetFrontTires(truck.transform.Find("FR Tire").gameObject.GetComponent<Tire>());
                playerTruckController.SetBackTires(truck.transform.Find("BL Tire").gameObject.GetComponent<Tire>());
                playerTruckController.SetBackTires(truck.transform.Find("BR Tire").gameObject.GetComponent<Tire>());
                playerTruckController.SetTireAngles();
                playerTruckController.SetDriverPos(truck.transform.Find("DriverPos").gameObject.transform);

                // change player input to car input
                playerTruckController.PlayerEnterTruck(player);
            }
        }
    }

    // Asks server very nicely if client can get out whip
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestGetOutTruck(NetworkId _playerId)
    {
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();

        player.truckController.enabled = false;

        // Add delay to player being able to enter so player doesn't instantly leave off of exit input
        player.canEnter = false;
        player.Invoke("EnterDelay", 0.5f);
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
            player.carriedObject.transform.DOShakeScale(0.5f, 0.8f);
        }
    }

    // Asks server very nicely if client can drop object
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestDrop(NetworkId _playerId)
    {
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();
        if (player.IsHolding() && player.carriedObject.GetCarrierId() == _playerId)
        {
            if (player.carriedObject.interactableType == InteractableType.Barrel)
            {
                player.carriedObject.Drop();
                Runner.Despawn(player.carriedObject.gameObject.GetComponent<NetworkObject>());
                player.carriedObject = null;
            }
            else 
            {
                player.carriedObject.Drop();
                if (player.carriedObject.interactableType == InteractableType.Flamethrower)
                {
                    player.carriedObject.gameObject.GetComponent<Flamethrower>().StopSpraying();
                }
                player.carriedObject.gameObject.GetComponent<NetworkObject>().RemoveInputAuthority();
                player.carriedObject = null;
            }
            
        }
    }

    //disable playercontrol, player kicked, arc(dynamc, v2(lerp)) vector getdistance, land, offset, vertically < 0.5, event stun, player land trigger, designer tools :3
    public void ExplosionHit(Vector3 direction, float distance)
    {
        Vector3 totalForce = direction.normalized * ((4f / distance) * 10);
        totalForce.y = 0;

        forceVec = totalForce;
        boosh = true;
    }

    private float time = 0;
    private bool boosh = false;
    private Vector3 forceVec = Vector3.zero;
    public void SimulateExplosion()
    {
        // ExplosionForce = 1/(1+(t^2 * 10))
        // steep bell curve at 0

        float explosionForce = (1f / (1 + (Mathf.Pow(time, 2) * 10)));

        Vector3 force = forceVec;

        force = force * explosionForce;
        Debug.Log(force);
        if (time == 0) 
        {
            gameObject.GetComponent<SimpleKCC>().Move(Vector3.zero, 10);
        }
        gameObject.GetComponent<SimpleKCC>().Move(force);

        if (time > 0.33f || (isGrounded && time > 0.1f))
        {
            time = 0;
            boosh = false;
        }

        time += Time.deltaTime;
    }


    public bool IsHolding()
    {
        return carriedObject != null;
    }

    private void EnterDelay()
    {
        canEnter = true;
    }

    private bool CanPickup(GameObject _interactable)
    {
        return _interactable.gameObject.tag == "FireHoseHead" || _interactable.gameObject.tag == "Barrel";
    }
}
