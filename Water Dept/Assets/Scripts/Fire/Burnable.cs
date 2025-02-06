using System.Collections;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class Burnable : MonoBehaviour
{
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST
    
    [SerializeField] protected float startFuel = 10.0f;
    private float fuelRemaining;
    [SerializeField] protected float spreadRate = 3.0f;
    [SerializeField] protected float spreadRadius = 10.0f;
    [SerializeField] protected float fireResistance = 3.0f;

    [SerializeField] protected bool isBurning = true;

    //prevents multiple fire sources from damaging an object separately
    private float lockoutTimer = 0.0f;
    private const float LOCKOUT_MAX_TIME = 3.0f;

    //lerp burn colors
    private Color startColor = Color.yellow;
    private Color endColor = Color.red;

    //temp vars for testing
    float timer = 0.0f;

    private void Start()
    {
        //used for tracking percentages
        fuelRemaining = startFuel;
        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    private void Update()
    {
        handeFire();
    }

    //does all functions related to burning of objects
    private void handeFire()
    {
        if(isBurning )
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

    //WIP
    IEnumerator SpreadFire()
    {
        yield return null;

        //spherecast here---
        if (Physics.SphereCast(gameObject.transform.position, spreadRadius, transform.forward, out RaycastHit hit, 10))
        {
            Debug.Log(hit);
        }
        else
        {
            Debug.Log("none");
        }
        
    }

    // Checks if burning object is on fire and burns it and spreads fire
    //Destroys object if no more fuel
    private void handleBurn()
    {
        //temp
        timer += Time.deltaTime;

        if (isBurning && fuelRemaining > 0)
        {
            fuelRemaining -= Time.deltaTime;

            if (timer > spreadRate)
            {
                tempSpread();
                timer = 0.0f;
            }

        }
        else if (fuelRemaining < 0)
        {
            Debug.Log("Object Destroyed");
            Destroy(gameObject);
        }
        else if (!isBurning)
        {
            //runs lockout timer down
            lockoutTimer -= Time.deltaTime;
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
    int result = 0;
    //handles damage/burn, goes through fire resistance first
    private void fireDamage(Burnable obj)
    {
        result = Random.Range(0, 2);
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
                Debug.Log(result);
                if (result >= 1)
                {
                    obj.isBurning = true;
                }
                else
                {
                    obj.fireResistance++;
                }
            }
        }
        
    }
}
