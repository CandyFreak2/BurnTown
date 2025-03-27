using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Addons.Physics;
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
    [SerializeField] private NetworkRigidbody3D networkedCarRB;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float gravityStrength = 12.0f;
    public Transform seatTransform;

    [SerializeField] private List<Tire> frontTires, backTires;
    public float tireMaxAngle = 70.0f;
    public float tireTurnSpeed = 1;
    //private float tireAngle = 0.0f;
    private Vector3 tireAngle;
    public Vector3 leftTireAngle, rightTireAngle;

    private float accelerationInput;
    private float brakeInput;
    [Networked] private float steerInput { get; set; }

    [Networked] private NetworkButtons previousButtons { get; set; }
    PlayerInputAction action;
    InputSystem_Actions.PlayerActions playerInputSystem; // raw input for driver
    Vector2 steerDirection;

    public PlayerMovement playerController;
    public bool isDriver;
    private bool canExit = false;
    private CarData[] trucks;
    private int closestTruckIndex = 0;

    private void Awake()
    {
        playerInputSystem = new InputSystem_Actions().Player;

        playerInputSystem.Move.performed += ctx => steerDirection = ctx.ReadValue<Vector2>();
        playerInputSystem.Move.canceled += ctx => steerDirection = ctx.ReadValue<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerMovement>();
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            //FreeCameraController.Singleton.SetTarget(camTarget);
            //GetComponentInChildren<CineSingleCamera>().SetAsActiveCamera();
            trucks = FindObjectsByType<CarData>(FindObjectsSortMode.None);
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
                //UpdateGravity();
            }
            else
            {
                CheckPlayerCount();
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
        //if (isDriver && HasInputAuthority)
        //{
        //    steerInput = playerInputSystem.Move.ReadValue<Vector2>().x;
        //}

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
            tireAngle = new Vector3(0.0f, Mathf.Lerp(0, -tireMaxAngle, Mathf.Abs(steerInput)), 0.0f);

            foreach (Tire tire in frontTires)
            {
                tire.transform.localEulerAngles = tireAngle;
            }
        }
        else if (steerInput > 0.0f)
        {
            tireAngle = new Vector3(0.0f, Mathf.Lerp(0, tireMaxAngle, Mathf.Abs(steerInput)), 0.0f);

            foreach (Tire tire in frontTires)
            {
                tire.transform.localEulerAngles = tireAngle;
            }
        }
        else
        {
            tireAngle = Vector3.zero;

            foreach (Tire tire in frontTires)
            {
                tire.transform.localEulerAngles = tireAngle;
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

    private void CheckPlayerCount()
    {
        if (carData.GetNumPassengers() == 1)
        {
            isDriver = true;
            seatTransform = carData.driverPos;
        }
    }

    public void PlayerEnterTruck(PlayerMovement _playerMovement)
    {
        isActive = true;
        playerController = _playerMovement;
        //playerController.DeactivateSystems();
        RPC_ActivatePlayerSystems(Object.Id, false);
        playerController.enabled = false;
        playerInputSystem.Enable();

        // Raise truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = 1;
        }

        // RB
        carRB.isKinematic = false;

        // Add delay to player being able to exit so player doesn't instantly leave off of enter input
        canExit = false;
        Invoke("ExitDelay", 0.5f);

        RPC_RequestPlayerEnterTruck(Object.Id, _playerMovement.Object.Id);
    }

    private void PlayerExitTruck()
    {
        carRB.isKinematic = true;

        // lower truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = -1;
        }

        playerController.enabled = true;
        RPC_ActivatePlayerSystems(Object.Id, true);
        if (seatTransform != null)
        {
            if (seatTransform.localPosition.x < 0)
            {
                playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position - seatTransform.right);
            }
            else
            {
                playerController.GetComponent<SimpleKCC>().SetPosition(seatTransform.position + seatTransform.right);
            }
        }

        carData.SubPassenger();
        RPC_RequestPlayerExitTruck(Object.Id);

        playerInputSystem.Disable();
        seatTransform = null;
        playerController.GetOutTruck();
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
            carController.playerController.enabled = false;


            // RB
            carController.carRB.isKinematic = false;

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
            carController.carRB.isKinematic = true;
            carController.playerController.enabled = true;
            carController.playerController.ReactivateSystems();
            if (carController.seatTransform != null)
            {
                if (carController.seatTransform.localPosition.x < 0)
                {
                    carController.playerController.GetComponent<SimpleKCC>().SetPosition(carController.seatTransform.position - carController.seatTransform.right);
                }
                else
                {
                    carController.playerController.GetComponent<SimpleKCC>().SetPosition(carController.seatTransform.position + carController.seatTransform.right);
                }
            }
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
        networkedCarRB = _data.networkCarRB;
        frontTires = _data.frontTires;
        backTires = _data.backTires;

        int numPassengers = _data.GetNumPassengers();
        if (numPassengers == 1)
        {
            isDriver = true;
            seatTransform = _data.driverPos;
        }
        else
        {
            isDriver = false;
            seatTransform = _data.passengerPos[numPassengers - 2];
        }
        SetTireAngles();
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

    public float GetDistanceToNearestTruck()
    {
        float minDistance = Vector3.Distance(trucks[0].truckPos, transform.position);

        for (int i = 0; i < trucks.Length; i++)
        {
            Mathf.Min(minDistance, Vector3.Distance(trucks[i].truckPos, transform.position));
            closestTruckIndex = i;
        }

        return minDistance;
    }

    public GameObject GetClosestTruck()
    {
        return trucks[closestTruckIndex].gameObject;
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

    public Rigidbody GetCarRB()
    {
        return carRB;
    }

    public NetworkRigidbody3D GetCarRBNetworked()
    {
        return networkedCarRB;
    }

    private void ExitDelay()
    {
        canExit = true;
    }
}
