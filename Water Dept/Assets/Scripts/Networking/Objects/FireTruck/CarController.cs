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
    private GameObject truckParent;
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform camTarget;
    private Transform driverPos;

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
    private PlayerMovement playerController;
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
        }
    }

    private void FixedUpdate()
    {
        if (playerController != null)
        {
            GameObject player = playerController.gameObject;

            player.transform.position = driverPos.position;
            player.transform.rotation = driverPos.rotation;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // gets input from server
        if (GetInput(out PlayerInputAction input))
        {
            action = input;

            GetTruckInput();
            TurnTires();
            UpdateTires();

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
                tire.transform.localRotation = Quaternion.LookRotation(Vector3.Slerp(carRB.transform.forward, leftTireAngle, Mathf.Abs(steerInput)), carRB.transform.up);

                //if (tireAngle > -140)
                //{
                //    tire.transform.Rotate(0, -tireTurnSpeed, 0);
                //    tireAngle -= tireTurnSpeed;
                //}
            }
        }
        else if (steerInput > 0.0f)
        {
            foreach (Tire tire in frontTires)
            {
                tire.transform.localRotation = Quaternion.LookRotation(Vector3.Slerp(carRB.transform.forward, rightTireAngle, Mathf.Abs(steerInput)), carRB.transform.up);

                //if (tireAngle < 140)
                //{
                //    tire.transform.Rotate(0, tireTurnSpeed, 0);
                //    tireAngle += tireTurnSpeed;
                //}
            }
        }
        else
        {
            foreach (Tire tire in frontTires)
            {
                tire.transform.forward = carRB.transform.forward;

                //if (tireAngle < 140)
                //{
                //    tire.transform.Rotate(0, tireTurnSpeed, 0);
                //    tireAngle += tireTurnSpeed;
                //}
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

    public void PlayerEnterTruck(PlayerMovement _playerMovement)
    {
        playerController = _playerMovement;
        playerController.DeactivateSystems();
        playerController.enabled = false;

        // Raise truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = 1;
        }

        // RB
       // carRB.isKinematic = false;
        Debug.Log("U in the shit bruh");

        // Add delay to player being able to exit so player doesn't instantly leave off of enter input
        canExit = false;
        Invoke("ExitDelay", 0.5f);

        RPC_RequestPlayerEnterTruck(Object.Id, _playerMovement.Object.Id);
    }

    private void PlayerExitTruck()
    {
        //carRB.isKinematic = true;

        // lower truck camera priority
        if (HasInputAuthority)
        {
            CinemachineCamera camera = truckParent.GetComponentInChildren<CinemachineCamera>();
            camera.Priority = -1;
        }

        //truckParent.GetComponent<Collider>().transform.localPosition = Vector3.zero;

        playerController.enabled = true;
        playerController.ReactivateSystems();
        playerController.GetComponent<SimpleKCC>().SetPosition(driverPos.position - driverPos.right);

        RPC_RequestPlayerExitTruck(Object.Id);

        playerController.GetOutTruck();
        playerController = null;
    }

    // Asks server very nicely if client can get in whip
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Only run by host
    public void RPC_RequestPlayerEnterTruck(NetworkId _truckId, NetworkId _playerId)
    {
        CarController carController = Runner.FindObject(_truckId).GetComponent<CarController>();
        PlayerMovement player = Runner.FindObject(_playerId).GetComponent<PlayerMovement>();

        if (carController != null && player != null)
        {
            carController.playerController = player;
            carController.playerController.DeactivateSystems();
            carController.playerController.enabled = false;


            // RB
            //carController.carRB.isKinematic = false;
            Debug.Log("U in the shit bruh");

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
            carController.playerController.enabled = true;
            carController.playerController.ReactivateSystems();
            carController.playerController.GetComponent<SimpleKCC>().SetPosition(driverPos.position - driverPos.right);
            //carController.playerController.GetOutTruck();
            //carController.playerController = null;
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

    public void SetDriverPos(Transform _driverPos)
    {
        driverPos = _driverPos;
    }

    private void ExitDelay()
    {
        canExit = true;
    }
}
