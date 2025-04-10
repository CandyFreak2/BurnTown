using DG.Tweening;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Burnable : NetworkBehaviour
{

    [Header("Score")]
    [SerializeField] private int burnableScore = 3500;
    //SYSTEM HALF DESIGNED FOR POLYMORPHISM
    //NOT IMPLEMENTED YET, JUST CONCEPT TEST

    [Header("Burning Effects")]
    [SerializeField, Networked] private int fuelRemaining { get; set; }

    [SerializeField, Tooltip("Increased by non-burnables like the flamethrower, decreases by 1 each seccond")]
    protected int heatModifier = 0;

    public int CurrentHeat { get => GetCurrentHeat(); }
    public List<Burnable> HeatSources = new List<Burnable>();

    [SerializeField, Tooltip("Does this object start on fire")]
    protected bool burnOnAwake = false;

    [Header("Heath and Resistance")]
    [SerializeField, Tooltip("Where does the fire spread from")]
    protected Transform spreadSource;

    [SerializeField, Tooltip("Where does fire spread to this object target")]
    protected Transform receiverPoint;

    [SerializeField, Tooltip("The health of the object, takes damage from surrounding fire and itself")] 
    protected int startFuel = 10;

    [SerializeField, Tooltip("The level of heat required to do damage to this object")] 
    protected int heatResistance = 0;

    [SerializeField, Tooltip("Heatbuffer takes damage before fuel does, slows down spread")]
    protected int heatBuffer = 5;

    [SerializeField, Tooltip("Max damage an object can take per heartbeat")]
    protected int heatDmgCap = 999;

    [Header("Damage and Spread")]
    [SerializeField, Tooltip("The amount of heat damage this object does to others, cumulative for each object")] 
    protected int heatStrength = 1;

    [SerializeField, Tooltip("The radius the fire will spread")]
    protected float spreadRadius = 10.0f;

    [SerializeField, Tooltip("The amount of heat damage this object does to others on it's final burst")]
    protected int finalHeatStrength = 2;

    [SerializeField, Tooltip("The radius the fire will spread on it's final burst")]
    protected float finalSpreadRadius = 0f;

    [SerializeField, Tooltip("How many walls the fire can penetrate before begin blocked")]
    protected int wallPenetration = 0;

    [SerializeField, Tooltip("How many walls the fire can penetrate before begin blocked")]
    protected LayerMask overlapMask;

    [SerializeField, Tooltip("The delay between each damage and spread tick, keep at 1 for now")] 
    protected static int heartBeat = 1;
    private float spreadTimer = 0.0f;

    //networked vars are communicated and updated by networkbehaviors
    [Header("Network")]
    [Networked] public bool nIsBurning { get; set; }
    bool startedBurning = false;
    bool stoppedBurning = false;

    private bool isSpawned = false;

    [Header("Colors")]
    [SerializeField] private bool showsLerp = true;
    [SerializeField, Range(0f, 1f), Tooltip("Smoothes the color transition, Low values = smoother")] 
    private float colorStepSmoothing = 0.05f;
    [SerializeField, Range(0f, 1f), Tooltip("Reaches max color at this ratio of fuel remaining")]
    private float colorStepMax = 0.2f;

    [SerializeField] private Color initialColor = Color.green;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;
    private Renderer render;
    private float burnVisOpacity;

    [Header("VFX")]
    [SerializeField] VFXController vfxControler;
    [SerializeField] List<ParticleSystem> onFireVFX;

    [Header("Audio")]
    [SerializeField] AudioProfile audioProfile;
    [Serializable]
    private class AudioProfile
    {
        public float fadeDuration = 0f;
        public List<AudioClip> DestructSFX;
        public List<AudioClip> BurningSFX;
    }
    private AudioSource sfxSource;

    [Header("Destruction")]
    [SerializeField] bool despawnOnDeath = true;
    [SerializeField] bool destroyedByImpact;
    [SerializeField] float destroyedDuration;
    [SerializeField] Transform activeModel;
    [SerializeField] Transform activeModelSubAnchor;
    [SerializeField] Transform destroyedModel;
    [SerializeField] ParticleSystem destructionVFX;
    private bool startedCollapse;
    private bool finishedCollapse;

    //screenshake
    [SerializeField] ScreenShake screenShake;
    [Serializable] private class ScreenShake
    {
        public float intensity = 0f;
        public float duration = 0f;
    }

    private void OnValidate()
    {
        if (spreadSource == null)
            spreadSource = transform;
        if (receiverPoint == null)
            receiverPoint = spreadSource == null ? transform : spreadSource.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spreadSource.position, spreadRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spreadSource.position, finalSpreadRadius);
    }

    public override void Spawned()
    {
        fuelRemaining = startFuel;
        isSpawned = true;
        nIsBurning = burnOnAwake;

        //offset update tick, by upto 0.2 sec. Spreads updates out over multiple frames
        spreadTimer += 0.01f * (gameObject.GetInstanceID() % 20);

        //used for tracking percentages
        if (showsLerp)
        {
            render = GetComponentInChildren<Renderer>();
            render.material.color = initialColor;
        }
    }

    //cannot get fuel remaining. It does not exists locally. It belongs to the network and can only be gotten from a networked update
    /*private void Update()
    {
        Effects();
    }*/

    public override void FixedUpdateNetwork()
    {
        HandleBurn();
    }

    private void FixedUpdate()
    {
        Effects();
    }

    //changes obj color 
    private void Effects()
    {
        //if (isSpawned && nIsBurning)
        {
            //lerp between start and end color based on % of fuel remaining
            if(showsLerp)
            {
                if(render != null)
                    render.material.color = Color.Lerp(endColor, startColor, ((float)fuelRemaining / (float)startFuel));

                if (vfxControler != null)
                {
                    burnVisOpacity = Mathf.InverseLerp((float)startFuel , startFuel * colorStepMax, (float)fuelRemaining);
                    vfxControler.Opacity = Mathf.Lerp(vfxControler.Opacity, burnVisOpacity, colorStepSmoothing); //stupid, but adds nice smoothing
                    //Debug.Log("Setting Opacity " +  burnVisOpacity);
                    //Debug.Log("Receved Opacity " +  vfxControler.Opacity);
                }
            }
        }
    }

    /// <summary> </summary>

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
            RPC_BurningStartedSignal();

        if (!stoppedBurning && fuelRemaining <= 0)
            RPC_BurningStopped();
    }


    //gets an array of burnables in spread radius and damages them
    ///Gets the array of burnables in a radius and adds the source to them
    private void SpreadFire()
    {
        Collider[] objArr = Physics.OverlapSphere(spreadSource.position, spreadRadius, overlapMask);
        foreach (var obj in objArr)
        {
            if (!obj.gameObject.TryGetComponent(out Burnable check_1) && !obj.gameObject.TryGetComponent(out BarrelInfo check_2) )
                continue;

            //Skip obj if over pen limit, (!System will break if obj is using multiple colliders!) //always add +1 because it will usually hit itself
            Vector3 targetPos = check_1 == null ? obj.transform.position : check_1.receiverPoint.position;
            RaycastHit[] hits = Physics.RaycastAll(spreadSource.position, targetPos - spreadSource.position, Vector3.Distance(spreadSource.position, obj.transform.position));
            if (hits.Length > wallPenetration + 1)
            {
                Debug.DrawLine(spreadSource.position, targetPos, Color.black, 2f);
                continue;
            }

            Debug.DrawLine(spreadSource.position, targetPos, Color.yellow, 2f);

            if (obj.TryGetComponent(out Burnable target))
            {
                //DealFireDamage(obj.GetComponent<Burnable>());
                target.enabled = true;
                target.AddHeat(Runner.FindObject(Object.Id).GetComponent<Burnable>());
            }

            if (obj.TryGetComponent(out BarrelInfo barrel))
            {
                barrel.Explode();
            }
        }
    }

    private void FinalSpread()
    {
        Collider[] objArr = Physics.OverlapSphere(spreadSource.position, finalSpreadRadius, overlapMask);
        foreach (var obj in objArr)
        {
            //screenshake
            if(obj.tag == "Player" && obj.GetComponentInParent<PlayerMovement>() != null && obj.GetComponentInParent<PlayerMovement>().HasInputAuthority)
            {
                Debug.DrawLine(spreadSource.position, obj.transform.position, Color.magenta);
                float shakeMultiplier = Mathf.InverseLerp(finalSpreadRadius, 0, Vector3.Distance(obj.transform.position, spreadSource.position));
                CinemachineShake.ShakeAllCameras(screenShake.intensity * shakeMultiplier, screenShake.duration * shakeMultiplier);
                continue;
            }

            //stop if object isnt burnable or explosive
            if (!obj.gameObject.TryGetComponent(out Burnable check_1) && !obj.gameObject.TryGetComponent(out BarrelInfo check_2))
                continue;

            //Skip obj if over pen limit, (!System will break if obj is using multiple colliders!) //always add +1 because it will usually hit itself
            Vector3 targetPos = check_1 == null ? obj.transform.position : check_1.receiverPoint.position;
            RaycastHit[] hits = Physics.RaycastAll(spreadSource.position, targetPos - spreadSource.position, Vector3.Distance(spreadSource.position, obj.transform.position));
            if (hits.Length > wallPenetration + 1)
            {
                Debug.DrawLine(spreadSource.position, targetPos, Color.black, 2f);
                continue;
            }

            Debug.DrawLine(spreadSource.position, obj.transform.position, Color.red, 2f);

            if (obj.TryGetComponent(out Burnable target))
            {
                target.enabled = true;
                target.AddHeatModifier(finalHeatStrength);
            }
        }
    }
    private void TakeFireDamageTick()
    {
        //Debug.Log(gameObject.name + "'s current heat is " + CurrentHeat);
        if(CurrentHeat <= heatResistance)
            return;

        int heatDamage = Mathf.Clamp(CurrentHeat - heatResistance, 0, heatDmgCap) * heartBeat;

        if(heatBuffer > 0)
            heatBuffer -= heatDamage;
        else
            fuelRemaining -= heatDamage;

        heatModifier -= heartBeat;
        heatModifier = Mathf.Clamp(heatModifier, 0, heatDmgCap);
    }
    int GetCurrentHeat()
    {
        int heat = 0;

        //sets heat to the hottest current object
        foreach (Burnable hs in HeatSources)
            heat += hs.heatStrength;

        //add the modifier from non-burnable sources like the flamethrower
        heat += heatModifier;

        return heat;
    }

    //
    // NETWORKED HEAT 
    //

    ///<summary>A function for taking damage from incoming fire sources</summary>  -James 03/08
    public void AddHeat(Burnable source)
    {
        if(!HeatSources.Contains(source))
        {
            //Debug.Log("Adding heat source");
            RPC_AddHeat(Object.Id, source);
            StartCoroutine(RemoveHeat(source));
        }
    }

    /// Tell everyone to add the new heatsource to their lists
    [Rpc(RpcSources.All, RpcTargets.All)] 
    private void RPC_AddHeat(NetworkId _objID, Burnable source)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.HeatSources.Add(source);
    }

    private IEnumerator RemoveHeat(Burnable source)
    {
        yield return new WaitForSeconds(heartBeat + 1);
        if(source != null)
            RPC_RemoveHeat(Object.Id, source);
    }
    
    /// Tell everyone that the heat source has expired
    [Rpc(RpcSources.All, RpcTargets.All)] 
    private void RPC_RemoveHeat(NetworkId _objID, Burnable source)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.HeatSources.Remove(source);
    }

    /// <summary>Add a heat modifier from non-burnable objects. Highest given value is used. Decreases by 1 every seccond</summary>
    public void AddHeatModifier(int heatValue)
    {
        if (heatModifier > heatValue)
            return;

        RPC_AddHeatModifier(Object.Id, heatValue);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_AddHeatModifier(NetworkId _objID, int heatValue)
    {
        Burnable target = Runner.FindObject(_objID).GetComponent<Burnable>();
        target.heatModifier = heatValue;
    }



    //gives host the authority on what burns
    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_BurningStartedSignal(RpcInfo info = default) => RPC_BurningStartedRelay(info.Source);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_BurningStartedRelay(PlayerRef messageSource)
    {
        //SHOULD NOT CALL RPC LIKE THIS BUT NETWORKED OBJ WONT TAKE MY NICE LIST   ):<
        startedBurning = true;
        nIsBurning = true;

        //Burning loop sfx started fade in set AudioSource to var
        sfxSource = AudioPlayer.PlaySFXLoop(audioProfile.BurningSFX, transform.position);
        StartCoroutine(AudioPlayer.FadeAudioIn(sfxSource, audioProfile.fadeDuration));

        //vfx is currently split in an ugly way. Here and in the controller :P
        foreach (var fire in onFireVFX)
            fire.Play();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_BurningStopped() => Runner.FindObject(Object.Id)?.GetComponent<Burnable>().BurningStopped();
    private void BurningStopped() //do not call, use RPC
    {
        //Burning sfx fade out and pass AudioSource to destroy it
        StartCoroutine(AudioPlayer.FadeAudioOut(sfxSource, audioProfile.fadeDuration));

        stoppedBurning = true;
        TriggerDestroyBurnable();
    }

    //
    // DESTRUCTION 
    //

    /// <summary>Trigger the destruction of the object. It will try to collapse unless it is set to despawn, but only if the destroyedByRamming bool is true</summary>
    /// <param name="overrideImpactSettings"> ignore objects impact settings -James</param>
    public void DestroyObjectImpact(bool overrideImpactSettings = false) => DestroyObjectImpact(destroyedDuration, overrideImpactSettings);
    /// <summary>Trigger the destruction of the object. The duration is how long it will crumble for</summary>
    public void DestroyObjectImpact(float duration, bool overrideImpactSettings = false)
    {
        if (destroyedByImpact || overrideImpactSettings)
        {
            RPC_TriggerDestroySignal(duration);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_TriggerDestroySignal(float duration, RpcInfo info = default) => RPC_TriggerDestroyRelay(duration, info.Source);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_TriggerDestroyRelay(float duration, PlayerRef messageSource)
    {
        TriggerDestroyBurnable(duration);
    }

    private void TriggerDestroyBurnable() => TriggerDestroyBurnable(destroyedDuration);
    private void TriggerDestroyBurnable(float duration)
    {
        //detatch VFX
        foreach (var fire in onFireVFX)
        {
            fire.Stop();
            Destroy(fire.gameObject, 10); //magic 10, VFX should be stopped and destroyed waaaaaayyy after they're done, but it's hard to tell when that is
            fire.transform.parent = null;
        }

        //this will not work if triggered by RPC call... ugggghhhh
        nIsBurning = false;


        //Destruction sfx
        AudioPlayer.PlaySFX(audioProfile.DestructSFX, transform.position);

        if (despawnOnDeath)
            TriggerDespawn();
        else
            TriggerCollapse(duration);
    }
    
    //it's annoying to have to delay this but the client fucks up without it. We should avoid removing objects from scene in the future
    private void TriggerDespawn()
    {
        EventManager.BurnableDestroyed(burnableScore);
        Invoke("Despawn", 0.2f);
    }
    private void Despawn()
    {
        if (destructionVFX != null)
        {
            destructionVFX.Play();
            destructionVFX.transform.parent = null;
        }
        
        Runner.Despawn(Object);
    }

    public void TriggerCollapse(float duration)
    {
        if(startedCollapse) 
            return;

        startedCollapse = true;
        FinalSpread();
        StartCoroutine(Collapse(duration));
    }

    private IEnumerator Collapse(float duration)
    {
        duration = Mathf.Clamp(duration, 1.2f, Mathf.Infinity);

        EventManager.BurnableDestroyed(burnableScore);
        destroyedModel.gameObject.SetActive(true);

        //play vfx and shake for 1 sec, also disable hitbox,
        GetComponent<Collider>().enabled = false;
        if (destructionVFX != null)
        {
            destructionVFX.Play();
        }
        activeModelSubAnchor.DOShakePosition(duration, 0.2f, 20, 90, false);
        yield return new WaitForSeconds(1f);

        //Collapse,  and wait untill finished
        activeModel.DOLocalMove(Vector3.down * 14f, duration);
        yield return new WaitForSeconds(duration);

        //disable this script and the model
        finishedCollapse = true;
        activeModel.gameObject.SetActive(false);
        enabled = false;
    }

    public bool GetDisabled(float _disabledPercent) {
        return isSpawned && _disabledPercent > fuelRemaining / startFuel;
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
