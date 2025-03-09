using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.XR;
using static Fusion.NetworkBehaviour;

public class Burnable : NetworkBehaviour
{

    [Header("Score")]
    [SerializeField] private int burnableScore = 3500;
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST

    [Header("Burning Effects")]
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

    [SerializeField, Tooltip("The delay between each damage and spread tick, low numbers may cause server lag")] 
    protected static int heartBeat = 1;

    [SerializeField, Tooltip("The radius the fire will spread")] 
    protected float spreadRadius = 10.0f;

    private float spreadTimer = 0.0f;

    //networked vars are communicated and updated by networkbehaviors
    [Header("Network")]
    [Networked] public bool nIsBurning { get; set; }
    bool startedBurning = false;
    bool stoppedBurning = false;

    private bool isSpawned = false;

    [Header("Colors")]
    [SerializeField] private bool showsLerp = true;
    [SerializeField] private Color initialColor = Color.green;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;
    private Renderer render;

    [Header("VFX")]
    [SerializeField] ParticleSystem onFireVFX;


    public override void Spawned()
    {
        fuelRemaining = startFuel;
        isSpawned = true;
        nIsBurning = burnOnAwake;

        //used for tracking percentages
        if (showsLerp)
        {
            render = GetComponent<Renderer>();
            render.material.color = initialColor;
        }
    }

    private void Update()
    {
        Effects();
    }

    public override void FixedUpdateNetwork()
    {
        HandleBurn();
    }


    //changes obj color 
    private void Effects()
    {
        if (isSpawned && nIsBurning)
        {
            //lerp between start and end color based on % of fuel remaining
            if(showsLerp)
                render.material.color = Color.Lerp(endColor, startColor, ((float)fuelRemaining / (float)startFuel));
        }
    }

    //Checks if burning object is on fire and burns it and spreads fire
    //Destroys object if no more fuel
    private void HandleBurn()
    {
        spreadTimer += Time.fixedDeltaTime;

        if (spreadTimer < heartBeat)
            return;

        spreadTimer = 0.0f;

        if (nIsBurning && fuelRemaining > 0)
        {
            AddHeat(this);
            SpreadFire();
        }

        TakeFireDamageTick();

        if(!startedBurning && heatBuffer <= 0 && fuelRemaining < startFuel)
            RPC_BurningStarted();

        if (!stoppedBurning && fuelRemaining <= 0)
            RPC_BurningStopped();
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
                target.enabled = true;
                target.AddHeat(Runner.FindObject(Object.Id).GetComponent<Burnable>());
            }
        }
    }

    

    private void TakeFireDamageTick()
    {
        //Debug.Log(gameObject.name + "'s current heat is " + CurrentHeat);
        if(CurrentHeat <= heatResistance)
            return;

        int heatDamage = Mathf.Clamp(CurrentHeat - heatResistance, 0, 9999) * heartBeat;

        if(heatBuffer > 0)
            heatBuffer -= heatDamage;
        else
            fuelRemaining -= heatDamage;
    }
    int GetCurrentHeat()
    {
        int heat = 0;

        //sets heat to the hottest current object
        foreach (Burnable hs in HeatSources)
            if (hs.heatStrength > heat)
                heat = hs.heatStrength;

        return heat;
    }

    ///A function for taking damage from incoming fire sources -James 03/08
    public void AddHeat(Burnable source)
    {
        if(!HeatSources.Contains(source))
        {
            //Debug.Log("Adding heat source");
            RPC_AddHeat(Object.Id, source);
            StartCoroutine(RemoveHeat(source));
        }
    }

    private IEnumerator RemoveHeat(Burnable source)
    {
        yield return new WaitForSeconds(heartBeat + 1);
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

    public void TriggerBurn() => RPC_TrigggerBurn(Object.Id);
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_TrigggerBurn(NetworkId _objID)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.nIsBurning = true;
    }


    [Rpc(RpcSources.All, RpcTargets.All)] 
    private void RPC_BurningStarted() => Runner.FindObject(Object.Id)?.GetComponent<Burnable>().BurningStarted();
    private void BurningStarted() //do not call, use RPC
    {
        startedBurning = true;
        nIsBurning = true;
        onFireVFX.Play();
    }


    [Rpc(RpcSources.All, RpcTargets.All)] 
    private void RPC_BurningStopped() => Runner.FindObject(Object.Id)?.GetComponent<Burnable>().BurningStopped();
    private void BurningStopped() //do not call, use RPC
    {
        stoppedBurning = true;

        //detatch VFX
        onFireVFX.Stop();
        Destroy(onFireVFX.gameObject, 10); //magic 10, VFX should be stopped and destroyed waaaaaayyy after they're done, but it's hard to tell when that is
        onFireVFX.transform.parent = null;

        nIsBurning = false;

        //it's annoying to have to delay this but the client fucks up without it. We should avoid removing objects from scene in the future
        Invoke("Despawn", 0.5f);
    }

    private void Despawn()
    {
        //despawn obj 
        EventManager.BurnableDestroyed(burnableScore);
        gameObject.SetActive(false);
        Runner.Despawn(Object);
    }

    public bool GetDisabled(float _disabledPercent)
    {
        return _disabledPercent > fuelRemaining / startFuel;
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


    // Checks if burn progress is below certain amount

}
