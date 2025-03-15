using Fusion;
using Fusion.Addons.SimpleKCC;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CarController : NetworkBehaviour
{
    private bool isSpawned = false;
    [Networked] public NetworkBool isActive { get; set; } // If player not in car isActive is false

    private CarData carData;
    public GameObject truckParent;
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float gravityStrength = 12.0f;
    public Transform seatTransform;
    [Networked] public Vector3 playerPos { get; set; }

    [SerializeField] private List<Tire> frontTires, backTires;
    public float tireMaxAngle = 70.0f;
    public float tireTurnSpeed = 1;
    private float tireAngle = 0.0f;
    public Vector3 leftTireAngle, rightTireAngle;

    private float accelerationInput;
    private float brakeInput;
    private float steerInput;

    [Networked] private NetworkButtons previousButtons { get; set; }
    PlayerInputAction action;
    
    public PlayerMovement playerController;
    private bool isDriver;
    private bool canExit = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            //FreeCameraController.Singleton.SetTarget(camTarget);
            //GetComponentInChildren<CineSingleCamera>().SetAsActiveCamera();
            playerController = GetComponent<PlayerMovement>();
            isSpawned = true;
        }
    }

    private void FixedUpdate()
    {
        if (isSpawned && isActive)
        {

            transform.localPosition = Vector3.zero/*truckParent.transform.position + truckParent.transform.TransformDirection(playerPos)*/;
            //player.transform.rotation = playerPos.rotation;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // gets input from server
        if (isActive && GetInput(out PlayerInputAction input))
        {
            transform.localPosition = Vector3.zero;

            action = input;

            GetTruckInput();

            if (isDriver)
            {
                TurnTires();
                UpdateTires();
                UpdateGravity();
            }

            previousButtons = action.Buttons;
        }
    }

    private void GetTruckInput()
    {
        // Acceleration
        if (action.Buttons.WasPressed(previousButtons, InputButton.Shoot))
        {
            accelerationInput = 1.0f;
        }
        else if(action.Buttons.WasReleased(previousButtons, InputButton.Shoot))
        {
            accelerationInput = 0.0f;
        }

        // Braking
        if (action.Buttons.WasPressed(previousButtons, InputButton.Brake))
        {
            brakeInput = 1.0f;
        }
        else if (action.Buttons.WasReleased(previousButtons, InputButton.Brake))
        {
            brakeInput = 0.0f;
        }

        steerInput = action.MoveDirection.x;

        // Player controls
        if (action.Buttons.WasPressed(previousButtons, InputButton.Interact) && canExit)
        {
            PlayerExitTruck();
        }
    }

    private void TurnTires()
    {
        // Sorry Jake, I was getting spammed :( - Jerry
        //Debug.Log(steerInput);
        if (steerInput < 0.0f)
        {
            foreach (Tire tire in frontTires)
            {
                //tire.transform.localRotation = Quaternion.LookRotation(Vector3.Slerp(carRB.transform.forward, leftTireAngle, Mathf.Abs(steerInput)), carRB.transform.up);

                if (tireAngle > -(2 * tireMaxAngle))
                {
                    tire.transform.Rotate(0, -tireTurnSpeed, 0);
                    tireAngle -= tireTurnSpeed;
                }
            }
        }
        else if (steerInput > 0.0f)
        {
            foreach (Tire tire in frontTires)
            {
                //tire.transform.localRotation = Quaternion.LookRotation(Vector3.Slerp(carRB.transform.forward, rightTireAngle, Mathf.Abs(steerInput)), carRB.transform.up);

                if (tireAngle < (2 * tireMaxAngle))
                {
                    tire.transform.Rotate(0, tireTurnSpeed, 0);
                    tireAngle += tireTurnSpeed;
                }
            }
        }
        else
        {
            foreach (Tire tire in frontTires)
            {
                //tire.transform.rotation = carRB.transform.rotation;

                if (tireAngle > 0)
                {
                    tire.transform.Rotate(0, -tireTurnSpeed, 0);
                    tireAngle -= tireTurnSpeed;
                }
                else if (tireAngle < 0)
                {
                    tire.transform.Rotate(0, tireTurnSpeed, 0);
                    tireAngle += tireTurnSpeed;
                }
            }
        }
    }

    private void UpdateTires()
    {
        //Update tire forces
        foreach (Tire tire in frontTires)
        {
            tire.UpdateForces(accelerationInput, brakeInput, transform.forward);

            carRB.AddForceAtPosition(tire.GetForces(), tire.transform.position);
        }
        foreach (Tire tire in backTires)
        {
            tire.UpdateForces(accelerationInput, brakeInput, transform.forward);

            carRB.AddForceAtPosition(tire.GetForces(), tire.transform.position);
        }
    }

    private void UpdateGravity()
    {
        if (carRB != null)
        {
            carRB.AddForce(Vector3.down * gravityStrength);
        }
    }

    public void PlayerEnterTruck(PlayerMovement _playerMovement)
    {
        isActive = true;
        playerController = _playerMovement;
        playerController.DeactivateSystems();
        RPC_ActivatePlayerSystems(Object.Id, false);
        playerController.enabled = false;

        // Raise truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = 1;
        }

        // RB
        carRB.useGravity = false;

        // Add delay to player being able to exit so player doesn't instantly leave off of enter input
        canExit = false;
        Invoke("ExitDelay", 0.5f);

        RPC_RequestPlayerEnterTruck(Object.Id, _playerMovement.Object.Id);
    }

    private void PlayerExitTruck()
    {
        carRB.useGravity = true;

        // lower truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = -1;
        }

        //truckParent.GetComponent<Collider>().transform.localPosition = Vector3.zero;

        playerController.enabled = true;
        //playerController.ReactivateSystems();
        RPC_ActivatePlayerSystems(Object.Id, true);
        if (seatTransform.localPosition.x < 0)
        {
            playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position - seatTransform.right);
        }
        else
        {
            playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position + seatTransform.right);
        }

        carData.SubPassenger();
        RPC_RequestPlayerExitTruck(Object.Id);

        seatTransform = null;
        playerController.GetOutTruck();
        //playerController = null;
        isActive = false;
    }

    // Asks server very nicely if client can get in whip
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestPlayerEnterTruck(NetworkId _truckId, NetworkId _playerId)
    {
        CarController carController = Runner.FindObject(_truckId).GetComponent<CarController>();
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();

        if (carController != null && player != null)
        {
            carController.isActive = true;
            carController.playerController = player;
            carController.playerController.DeactivateSystems();
            //RPC_ActivatePlayerSystems(_playerId, false);
            carController.playerController.enabled = false;


            // RB
            //carController.carRB.useGravity = false;

            // Add delay to player being able to exit so player doesn't instantly leave off of enter input
            carController.canExit = false;
            carController.Invoke("ExitDelay", 0.5f);
        }
    }

    // Asks server very nicely if client can get in whip
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestPlayerExitTruck(NetworkId _truckId)
    {
        CarController carController = Runner.FindObject(_truckId).GetComponent<CarController>();

        if (carController != null)
        {
            carController.carRB.useGravity = true;
            carController.playerController.enabled = true;
            carController.playerController.ReactivateSystems();
            //RPC_ActivatePlayerSystems(_truckId, true);
            if (carController.seatTransform.localPosition.x < 0)
            {
                carController.playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position - seatTransform.right);
            }
            else
            {
                carController.playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position + seatTransform.right);
            }
            //carController.carData.SubPassenger();
            //carController.playerController.GetOutTruck();
            //carController.playerController = null;
            carController.seatTransform = null;
            carController.isActive = false;
        }
    }

    // Updates player activation to everyone
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ActivatePlayerSystems(NetworkId _playerId, bool _activate)
    {
        CarController carController = Runner.FindObject(_playerId).GetComponent<CarController>();

        if (_activate)
        {
            carController.playerController.ReactivateSystems();
        }
        else
        {
            carController.playerController.DeactivateSystems();
        }
    }

    public void SetData(CarData _data)
    {
        carData = _data;
        truckParent = _data.gameObject;
        carRB = _data.carRB;
        frontTires = _data.frontTires;
        backTires = _data.backTires;

        int numPassengers = _data.GetNumPassengers();
        if (numPassengers == 1)
        {
            isDriver = true;
            seatTransform = _data.driverPos;
            playerPos = seatTransform.localPosition;
        }
        else
        {
            isDriver = false;
            seatTransform = _data.passengerPos[numPassengers - 2];
            playerPos = seatTransform.localPosition;
        }
    }

    public void SetTireAngles()
    {
        Transform tmpTrans = frontTires[0].transform;
        tmpTrans.Rotate(0, -tireMaxAngle, 0);
        leftTireAngle = tmpTrans.transform.forward;
        tmpTrans.Rotate(0, 2 * tireMaxAngle, 0);
        rightTireAngle = tmpTrans.transform.forward;
        tmpTrans.Rotate(0, -tireMaxAngle, 0);

        Debug.DrawLine(carRB.transform.position, carRB.transform.position + leftTireAngle * 10, Color.red, 5.0f);
        Debug.DrawLine(carRB.transform.position, carRB.transform.position + rightTireAngle * 10, Color.red, 5.0f);
    }

    public void SetTruckParent(GameObject _truck)
    {
        truckParent = _truck;
    }

    public void SetCarRB(Rigidbody _rb)
    {
        carRB = _rb;
    }

    public void SetFrontTires(Tire _frontTire)
    {
        if (frontTires.Count < 2)
        {
            frontTires.Add(_frontTire);
        }
    }

    public void SetBackTires(Tire _backTire)
    {
        if (backTires.Count < 2)
        {
            backTires.Add(_backTire);
        }
    }

    //public void SetDriverPos(Transform _driverPos)
    //{
    //    playerPos = _driverPos;
    //}

    private void ExitDelay()
    {
        canExit = true;
    }
}
