using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Fusion;
using DG.Tweening;
using System;
using UnityEngine.InputSystem;

public class Flamethrower : NetworkBehaviour
{
    //[Networked] private NetworkBool isShooting { get; set; }
    private bool isShooting;

    /*[Header("Fuel")]
    [SerializeField] GameObject fuelPrefab;
    [SerializeField] float fuelSpawnDelay = 0.2f;
    float fuelTimer;*/

    [Header("Damage & Fire")]
    [SerializeField] int heatApplied = 1;

    [Header("Fuel")]
    [Tooltip("Max amount of fuel and starting fuel")]
    [SerializeField] public float startFuel = 10f;
    [Tooltip("Fuel amount flamethrower loses (seconds)"), Range(0, 3)]
    public float fuelDepletionRate = 1;
    [Tooltip("Fuel amount flamethrower regenerates (seconds)"), Range(0, 3)]
    public float fuelRegenerationRate = 1;
    //[Networked] public float fuel { get; set; }
    public float fuel;
    [SerializeField] private GameObject fuelBarElements;
    [SerializeField] private Transform fuelBarAnchor;
    [SerializeField] private MeshRenderer fuelBarMesh;

    [Header("Raycast")]
    [SerializeField] Transform raySource;
    [SerializeField] float rayAccuracy;
    [SerializeField] float rayDistance;
    [SerializeField] LayerMask rayMask;


    [Header("Tween")]
    [SerializeField] float tweenDuration;
    [SerializeField] Vector3 tweenMagnatude;
    [SerializeField] int tweenVibrato;
    Tween activeTween;

    [Header("VFX")]
    [SerializeField] private ParticleSystem VFX_Flame;
    [SerializeField] private ObjectPool decalPool;
    [SerializeField] float decalSpawnDelay = 0.2f;
    float decalTimer;

    [Header("Audio")]
    [SerializeField] AudioProfile audioProfile;
    [Serializable] private class AudioProfile
    {
        public float fadeDuration = 0.2f;
        public AudioClip flamethrowerLoop;
        public List<AudioClip> flamethrowerStart;
        [HideInInspector] public Coroutine activeFade;
        [HideInInspector] public AudioSource activeSource;
    }

    [Header("Audio")]
    [SerializeField] float rumbleMultiplier = 0.5f;
    public override void Spawned()
    {
        isShooting = false;
        audioProfile.activeSource = AudioPlayer.PlaySFXLoop(audioProfile.flamethrowerLoop, transform, 0);
        fuel = startFuel;
        fuelBarMesh.material.color = Color.green;
        fuelBarElements.SetActive(false);
    }

    public void Spray()
    {
        fuelBarElements.SetActive(true);
        RPC_ShootFire(Object.Id);

        if(HasInputAuthority)
        {
            if (Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(0.25f * rumbleMultiplier, 0.75f * rumbleMultiplier);
                InputSystem.ResumeHaptics();
            }
        }
    }

    public void StopSpraying()
    {
        RPC_StopFire(Object.Id);
        if (HasInputAuthority)
        {
            InputSystem.PauseHaptics();
        }
    }

    public void FixedUpdate() //IS THIS STUPID? YES! DOES IT WORK? ALSO YES!
    {
        FuelUpdate();

        if (!isShooting)
            return;

        //fuelTimer += Time.fixedDeltaTime;
        decalTimer += Time.fixedDeltaTime;

        //give a random accuracy for this shot,      //will desync the players... ikd 
        raySource.localRotation = Quaternion.identity;
        raySource.Rotate(Vector3.forward, UnityEngine.Random.Range(0f, 360f), Space.Self);
        raySource.Rotate(Vector3.up, UnityEngine.Random.Range(0f, rayAccuracy), Space.Self);

        //ray cast the shot
        if (Physics.Raycast(raySource.position, raySource.forward, out RaycastHit hit, rayDistance, rayMask))
        {
            if(hit.collider.TryGetComponent(out Burnable burn))
            if (burn == null) 
            {
                burn = hit.collider.GetComponentInChildren<Burnable>(true);
            }


            //apply fire dammage to burnables
            if (burn != null)
            {
                if (!burn.enabled) burn.enabled = true;
                    burn.AddHeatModifier(heatApplied);
            }

            //apply fire dammage to barrels
            if(hit.collider.TryGetComponent(out BarrelInfo barrel))
            {
                barrel.Explode();
            }

            if(decalTimer > decalSpawnDelay)
            {
                //apply decals
                Transform decal = decalPool.GetPooledObject().transform;
                decal.position = hit.point;
                decal.forward = -hit.normal;
                decalTimer = 0f;
            }

        }

        if(fuel <= 0)
        {
            StopSpraying();
            fuelBarAnchor.localScale = Vector3.zero;
        }
    }

    // Tell everyone to burn shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_ShootFire(NetworkId _hoseId)
    {
        Flamethrower firehose = Runner.FindObject(_hoseId).GetComponent<Flamethrower>();
        firehose.isShooting = true;
        firehose.fuel = fuel;

        if (firehose != null)
        {
            //tween
            if (activeTween != null) activeTween.Complete();
            activeTween = firehose.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);

            //vfx start
            firehose.VFX_Flame.Play();

            //audio start
            AudioPlayer.PlaySFX(audioProfile.flamethrowerStart, transform);
            if (audioProfile.activeFade != null) StopCoroutine(audioProfile.activeFade);
            audioProfile.activeFade = StartCoroutine(AudioPlayer.FadeAudioIn(audioProfile.activeSource, audioProfile.fadeDuration));
        }
    }

    // Tell everyone to stop burning shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_StopFire(NetworkId _hoseId)
    {
        Flamethrower firehose = Runner.FindObject(_hoseId).GetComponent<Flamethrower>();
        firehose.isShooting = false;
        firehose.fuel = fuel;

        if (firehose != null)
        {
            //vfx stop
            firehose.VFX_Flame.Stop();

            //audio stop
            if (audioProfile.activeFade != null) StopCoroutine(audioProfile.activeFade);
            audioProfile.activeFade = StartCoroutine(AudioPlayer.FadeAudioOut(audioProfile.activeSource, audioProfile.fadeDuration));
        }
    }

    private void FuelUpdate()
    {
        //regen fuel
        fuel = isShooting ? fuel - Time.fixedDeltaTime * fuelDepletionRate : fuel + Time.fixedDeltaTime * fuelRegenerationRate;
        fuel = Mathf.Clamp(fuel, 0, startFuel);

        //lerp fuel indicators
        if (fuel > 0)
        {
            fuelBarAnchor.localScale = new Vector3(1, Mathf.Lerp(0, 1, fuel / startFuel), 1);
            fuelBarMesh.material.color = Color.Lerp(Color.red, Color.green, fuel / startFuel);
        }

        //disable when full
        if(fuel >= startFuel)
        {
            fuelBarElements.SetActive(false);
        }
    }
}
