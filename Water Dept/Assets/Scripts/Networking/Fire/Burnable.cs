using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.XR;
using static Fusion.NetworkBehaviour;

public class Burnable : NetworkBehaviour
{
    public bool burnOnAwake = false;

    // SCORE
    [SerializeField] private int burnableScore = 3500;
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST

    [SerializeField] protected int startFuel = 10;
    [SerializeField, Networked] private int fuelRemaining { get; set; }
    public int CurrentHeat { get => GetCurrentHeat(); }
    public List<Burnable> HeatSources = new List<Burnable>();
    [SerializeField] protected int heatResistance = 0;
    [SerializeField] protected int heatStrength = 1;

    [SerializeField] protected float spreadDelay = 1.0f;
    [SerializeField] protected float spreadRadius = 10.0f;
    private float spreadTimer = 0.0f;

    //networked vars are communicated and updated by networkbehaviors
    [Networked] public bool nIsBurning { get; set; }
    private bool isSpawned = false;
    [SerializeField] private bool showsLerp = true;

    //prevents multiple fire sources from damaging an object separately
    //private float lockoutTimer = 0.0f;
    //private const float LOCKOUT_MAX_TIME = 1.0f;

    //colors
    [SerializeField] private Color initialColor = Color.green;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;

    //VFX
    [SerializeField] ParticleSystem onFireVFX;

    public override void Spawned()
    {
        fuelRemaining = startFuel;
        isSpawned = true;
        nIsBurning = burnOnAwake;

        //used for tracking percentages
        if (showsLerp)
        {
            gameObject.GetComponent<Renderer>().material.color = initialColor;
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        handeColorLerp();
    }

    public override void FixedUpdateNetwork()
    {
        HandleBurn();
    }

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

        if(burnOnAwake)
            AddHeat(this);

        if(CurrentHeat > heatResistance)
        {
            nIsBurning = true;
            if(onFireVFX != null && !onFireVFX.isPlaying)
                onFireVFX.Play();
        }

        if (nIsBurning && fuelRemaining > 0)
        {
            if (spreadTimer > spreadDelay)
            {
                TempSpread();
                TakeFireDamageTick();
                spreadTimer = 0.0f;
            }

        }
        if (fuelRemaining <= 0)
        {
            //detatch VFX
            onFireVFX.transform.parent = null;
            onFireVFX.Stop();
            Destroy(onFireVFX, onFireVFX.main.duration);

            //disable obj
            gameObject.SetActive(false);
            EventManager.BurnableDestroyed(burnableScore);
            Runner.Despawn(Object);
        }
    }

    //gets an array of burnables in spread radius and damages them
    ///Gets the array of burnables in a radius and adds the source to them
    private void TempSpread()
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

        int heatDamage = CurrentHeat - heatResistance;
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
        yield return new WaitForSeconds(1f);
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
