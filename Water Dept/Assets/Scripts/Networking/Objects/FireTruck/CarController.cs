using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : NetworkBehaviour
{
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform camTarget;

    [SerializeField] private List<Tire> frontTires, backTires;
    public float tireTurnSpeed = 1;
    private float tireAngle = 0.0f;

    private float accelerationInput;
    private float brakeInput;
    private float steerInput;

    [Networked] private NetworkButtons previousButtons { get; set; }
    PlayerInputAction action;

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            FreeCameraController.Singleton.SetTarget(camTarget);
            //GetComponentInChildren<CineSingleCamera>().SetAsActiveCamera();
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
    }

    private void TurnTires()
    {
        if (steerInput < 0.0f)
        {
            foreach (Tire tire in frontTires)
            {
                if (tireAngle > -140)
                {
                    tire.transform.Rotate(0, -tireTurnSpeed, 0);
                    tireAngle -= tireTurnSpeed;
                }
            }
        }
        if (steerInput > 0.0f)
        {
            foreach (Tire tire in frontTires)
            {
                if (tireAngle < 140)
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

    // Update is called once per frame
    //private void Update()
    //{
    //    accelerationInput = Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f;

    //    brakeInput = Input.GetKey(KeyCode.DownArrow) ? 1.0f : 0.0f;

    //    if (Input.GetKey(KeyCode.LeftArrow))
    //    {
    //        foreach (Tire tire in frontTires)
    //        {
    //            if (tireAngle > -140)
    //            {
    //                tire.transform.Rotate(0, -tireTurnSpeed, 0);
    //                tireAngle -= tireTurnSpeed;
    //            }
    //        }
    //    }
    //    if (Input.GetKey(KeyCode.RightArrow))
    //    {
    //        foreach (Tire tire in frontTires)
    //        {
    //            if (tireAngle < 140)
    //            {
    //                tire.transform.Rotate(0, tireTurnSpeed, 0);
    //                tireAngle += tireTurnSpeed;
    //            }
    //        }
    //    }
    //}

    //private void FixedUpdate()
    //{
    //    //Update tire forces
    //    foreach (Tire tire in frontTires)
    //    {
    //        tire.UpdateForces(accelerationInput, brakeInput, transform.forward);

    //        carRB.AddForceAtPosition(tire.GetForces(), tire.transform.position);
    //    }
    //    foreach (Tire tire in backTires)
    //    {
    //        tire.UpdateForces(accelerationInput, brakeInput, transform.forward);

    //        carRB.AddForceAtPosition(tire.GetForces(), tire.transform.position);
    //    }
    //}
}
