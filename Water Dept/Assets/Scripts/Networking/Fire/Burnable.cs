using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.XR;
using static Fusion.NetworkBehaviour;

public class Burnable : NetworkBehaviour
{
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST

    [SerializeField] protected float startFuel = 10.0f;
    private float fuelRemaining;

    [SerializeField] protected float spreadRate = 3.0f;
    [SerializeField] protected float spreadRadius = 10.0f;
    [SerializeField] protected float fireResistance = 3.0f;
    //how much "health" damage a burnable takes per update if being actively burnt
    [SerializeField] protected float damageRate = 0.1f;

    //networked vars are communicated and updated by networkbehaviors
    [Networked] protected bool nIsBurning { get; set; }
    [SerializeField] protected bool isBurning = true;
    private bool isSpawned = false;

    //prevents multiple fire sources from damaging an object separately
    private float lockoutTimer = 0.0f;
    private const float LOCKOUT_MAX_TIME = 3.0f;

    //lerp burn colors
    private Color startColor = Color.yellow;
    private Color endColor = Color.red;

    //temp vars for testing
    float timer = 0.0f;

    public override void Spawned()
    {
        nIsBurning = isBurning;
        isSpawned = true;
    }

    private void Start()
    {
        //used for tracking percentages
        fuelRemaining = startFuel;
        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    private void Update()
    {
        if (isSpawned)
        {
            handeFire();
            isBurning = nIsBurning;
        }

    }

    //does all functions related to burning of objects
    private void handeFire()
    {
        if(nIsBurning)
        {
            handeColorLerp();
        }
        
        //SpreadFire();

        handleBurn();
    }

    //changes obj color 
    private void handeColorLerp()
    {
        //lerp between start and end color based on % of fuel remaining
        gameObject.GetComponent<Renderer>().material.color = Color.Lerp(endColor, startColor, (fuelRemaining / startFuel));
    }

    // Checks if burning object is on fire and burns it and spreads fire
    //Destroys object if no more fuel
    private void handleBurn()
    {
        //temp
        timer += Time.deltaTime;

        if (nIsBurning && fuelRemaining > 0)
        {
            fuelRemaining -= Time.deltaTime;

            if (timer > spreadRate)
            {
                tempSpread();
                timer = 0.0f;
            }

        }
        else if (!nIsBurning)
        {
            //runs lockout timer down
            lockoutTimer -= Time.deltaTime;

            //placeholder for updating req
            if (gameObject.GetComponent<Renderer>().material.color != Color.green)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            
        }
        else if (fuelRemaining < 0)
        {
            Debug.Log("Object Destroyed");
            //Destroy(gameObject);
            //Runner.Despawn(Object);
            gameObject.SetActive(false);
        }
    }

    //gets an array of burnables in spread radius and damages them
    private void tempSpread()
    {
        Collider[] objArr = Physics.OverlapSphere(gameObject.transform.position, spreadRadius);
        foreach(var obj in objArr)
        {
            if (obj.GetComponent<Burnable>() != null)
            {
                fireDamage(obj.GetComponent<Burnable>());
            }
        }
    }
    //handles damage/burn, goes through fire resistance first
    private void fireDamage(Burnable obj)
    {
        int result = Random.Range(0, 2);
        if (obj.lockoutTimer <= 0.0f)
        {
            if (obj.fireResistance > 0)
            {
                obj.fireResistance--;
                //resets an objects lockout timer to X
                obj.lockoutTimer = LOCKOUT_MAX_TIME;
            }
            else
            {
                if (result >= 1)
                {
                    obj.nIsBurning = true;
                }
                else
                {
                    obj.fireResistance++;

                }

            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Flame")
        {
            nIsBurning = true;
        }
    }

    // continually burning makes it burn faster based on its damageRate (applied every update)
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Flame" && nIsBurning)
        {
            fuelRemaining = fuelRemaining - damageRate;
        }
    }

    // Checks if burn progress is below certain amount
    public bool GetDisabled(float _disabledPercent)
    {
        return _disabledPercent > fuelRemaining / startFuel;
    }
}
