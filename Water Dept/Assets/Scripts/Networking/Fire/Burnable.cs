using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.XR;
using static Fusion.NetworkBehaviour;

public class Burnable : NetworkBehaviour
{

    // SCORE
    [SerializeField] private int burnableScore = 3500;
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST

    [SerializeField, Networked] private int fuelRemaining { get; set; }
    public int CurrentHeat { get => GetCurrentHeat(); }
    public List<Burnable> HeatSources = new List<Burnable>();

    [SerializeField, Tooltip("Does this object start on fire")]
    protected bool burnOnAwake = false;

    [SerializeField, Tooltip("The health of the object, takes damage from surrounding fire and itself")] 
    protected int startFuel = 10;

    [SerializeField, Tooltip("The level of heat required to do damage to this object")] 
    protected int heatResistance = 0;

    [SerializeField, Tooltip("The amount of heat damage this object does to others, only the highest is used")] 
    protected int heatStrength = 1;

    [SerializeField, Tooltip("Heatbuffer takes damage before fuel does")] 
    protected int heatBuffer = 5;

    [SerializeField, Tooltip("The delay between each damage and spread tick, sets the speed of the simulation, keep at 1 second ")] 
    protected float triggerDelay = 1.0f;

    [SerializeField, Tooltip("The radius the fire will spread")] 
    protected float spreadRadius = 10.0f;

    private float spreadTimer = 0.0f;

    //networked vars are communicated and updated by networkbehaviors
    [Networked] public bool nIsBurning { get; set; }
    private bool isSpawned = false;
    [SerializeField] private bool showsLerp = true;
    private bool overrideBurning;
    //prevents multiple fire sources from damaging an object separately
    //private float lockoutTimer = 0.0f;
    //private const float LOCKOUT_MAX_TIME = 1.0f;

    //colors
    [SerializeField] private Color initialColor = Color.green;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;

    //VFX
    [SerializeField] ParticleSystem onFireVFX;
    bool stopEffects = false;

    public override void Spawned()
    {
        fuelRemaining = startFuel;
        isSpawned = true;
        //nIsBurning = burnOnAwake;

        //used for tracking percentages
        if (showsLerp)
        {
            gameObject.GetComponent<Renderer>().material.color = initialColor;
        }
    }

    private void Start()
    {
        triggerDelay += Random.Range(-0.1f, 0.1f);
    }

    private void Update()
    {
        handeColorLerp();
    }

    public override void FixedUpdateNetwork()
    {
        HandleBurn();
    }
    public void TriggerBurn() => overrideBurning = true;

    //changes obj color 
    private void handeColorLerp()
    {
        if (isSpawned && nIsBurning)
        {
            //lerp between start and end color based on % of fuel remaining
            gameObject.GetComponent<Renderer>().material.color = Color.Lerp(endColor, startColor, ((float)fuelRemaining / (float)startFuel));
        }
    }

    // Checks if burning object is on fire and burns it and spreads fire
    //Destroys object if no more fuel
    private void HandleBurn()
    {
        //temp
        spreadTimer += Time.fixedDeltaTime;

        if (spreadTimer < triggerDelay)
            return;
        spreadTimer = 0.0f;

        if (burnOnAwake || overrideBurning)
            AddHeat(this);

        if(CurrentHeat > heatResistance)
        {
            TakeFireDamageTick();
            if(heatBuffer <= 0)
            {
                nIsBurning = true;
                if (onFireVFX != null && !onFireVFX.isPlaying)
                    onFireVFX.Play();
            }
        }

        if (nIsBurning && fuelRemaining > 0)
            SpreadFire();

        if(fuelRemaining <= 1 && !stopEffects)
        {
            stopEffects = true;

            //detatch VFX
            onFireVFX.transform.parent = null;
            onFireVFX.Stop();
            Destroy(onFireVFX.gameObject, onFireVFX.main.duration);
        }

        if (fuelRemaining <= 0)
        {
            nIsBurning = false;

            //despawn obj
            EventManager.BurnableDestroyed(burnableScore);
            gameObject.SetActive(false);
            Runner.Despawn(Object);
        }
    }


    //gets an array of burnables in spread radius and damages them
    ///Gets the array of burnables in a radius and adds the source to them
    private void SpreadFire()
    {
        Collider[] objArr = Physics.OverlapSphere(gameObject.transform.position, spreadRadius);
        foreach (var obj in objArr)
        {
            if (obj.TryGetComponent(out Burnable target))
            {
                //DealFireDamage(obj.GetComponent<Burnable>());
                target.AddHeat(Runner.FindObject(Object.Id).GetComponent<Burnable>());
            }
        }
    }

    /// Oh boy, this was alot to wrap my head arround, but I think I get it... Gotta make a way for the flamethrower to effect this -James 03/08
    //handles damage/burn, goes through fire resistance first
    /*private void DealFireDamage(Burnable obj)
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
                obj.nIsBurning = true;
                //if (result >= 1)
                //{
                //    obj.nIsBurning = true;
                //}
                //else
                //{
                //    obj.fireResistance++;

                //}

            }
        }

    }*/

    private void TakeFireDamageTick()
    {
        //Debug.Log(gameObject.name + "'s current heat is " + CurrentHeat);
        if(CurrentHeat <= heatResistance)
            return;

        int heatDamage = Mathf.Clamp(CurrentHeat - heatResistance, 0, 9999);

        if(heatBuffer > 0)
            heatBuffer -= heatDamage;
        else
            fuelRemaining -= heatDamage;
    }

    ///A function for taking damage from incoming fire sources (spisifically the player flamethrower) -James 03/08
    public void AddHeat(Burnable source)
    {
        if(!HeatSources.Contains(source))
        {
            //Debug.Log("Adding heat source");
            RPC_AddHeat(Object.Id, source);
            StartCoroutine(RemoveHeatSource(source));
        }
    }

    private IEnumerator RemoveHeatSource(Burnable source)
    {
        yield return new WaitForSeconds(triggerDelay);
        RPC_RemoveHeat(Object.Id, source);
    }

    /// Tell everyone to add the new heatsource to their lists
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    private void RPC_AddHeat(NetworkId _objID, Burnable source)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.HeatSources.Add(source);
    }

    /// Tell everyone that the heat source has expired
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    private void RPC_RemoveHeat(NetworkId _objID, Burnable source)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.HeatSources.Remove(source);
    }

    int GetCurrentHeat()
    {
        int heat = 0;

        //sets heat to the hottest current object
        foreach (Burnable hs in HeatSources)
            if(hs.heatStrength > heat)
                heat = hs.heatStrength;

        return heat;
    }

    /// removed, for flamethrower reworks -James 03\08
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Flame")
        {
            nIsBurning = true;
        }
    }

    // continually burning makes it burn faster based on its damageRate (applied every update)
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Flame" && nIsBurning)
        {
            fuelRemaining = fuelRemaining - damageRate;
        }
    }
    */

    // Checks if burn progress is below certain amount
    public bool GetDisabled(float _disabledPercent)
    {
        return _disabledPercent > fuelRemaining / startFuel;
    }
}
