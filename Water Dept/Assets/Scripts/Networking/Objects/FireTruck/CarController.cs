using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody carRB;

    [SerializeField]
    private List<Tire> frontTires, backTires;
    public float tireTurnSpeed = 1;
    private float tireAngle = 0.0f;

    private float accelerationInput;
    private float brakeInput;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        accelerationInput = Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f;

        brakeInput = Input.GetKey(KeyCode.DownArrow) ? 1.0f : 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow))
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
        if (Input.GetKey(KeyCode.RightArrow))
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

    private void FixedUpdate()
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
}
