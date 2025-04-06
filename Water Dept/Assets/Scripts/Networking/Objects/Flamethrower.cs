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
    [SerializeField] int heatAppliedDefault = 1;
    [SerializeField] int heatAppliedMax = 2;

    [Header("Fuel")]
    [Tooltip("Max amount of overcharge")]
    [SerializeField] public float maxOvercharge = 10f;
    [Tooltip("Temp amount flamethrower gains (seconds)"), Range(0, 3)]
    public float overchargeGainRate = 1;
    [Tooltip("Temp amount flamethrower cools (seconds)"), Range(0, 3)]
    public float overchargeCoolRate = 1;
    //[Networked] public float fuel { get; set; }
    public float overcharge;
    [SerializeField] private GameObject fuelBarElements;
    [SerializeField] private Transform fuelBarAnchor;
    [SerializeField] private MeshRenderer fuelBarMesh;

    [Header("Raycast")]
    [SerializeField] Transform raySource;
    [SerializeField] float rayAccuracy;
    [SerializeField] float rayDistance;
    [SerializeField] LayerMask rayMask;

/*
    [Header("Tween")]
    [SerializeField] float tweenDuration;
    [SerializeField] Vector3 tweenMagnatude;
    [SerializeField] int tweenVibrato;
    Tween activeTween;
*/

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
        overcharge = 0;
        fuelBarMesh.material.color = Color.green;
        fuelBarElements.SetActive(false);
    }

    public void Spray()
    {
        fuelBarElements.SetActive(true);
        RPC_ShootFire(Object.Id);

        if(HasInputAuthority)
        {
            if (Gamepad.current != null) 
            {
                if(overcharge >= maxOvercharge)
                    Gamepad.current.SetMotorSpeeds(0.25f * rumbleMultiplier, 0.75f * rumbleMultiplier);
                else
                    Gamepad.current.SetMotorSpeeds(0.15f * rumbleMultiplier, 0.5f * rumbleMultiplier);

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
            //apply fire dammage to burnables
            if (hit.collider.TryGetComponent(out Burnable burn))
            {
                burn.enabled = true;
                burn.AddHeatModifier(heatApplied);
            }

            //set off gassline
            if(hit.collider.GetComponent<GaslinePoint>())
            {
                hit.collider.GetComponent<GaslinePoint>().setFire();
            }

            //apply fire dammage to barrels
            if(hit.collider.TryGetComponent(out BarrelInfo barrel))
            {
                barrel.Explode();
            }

            //apply decals
            if(decalTimer > decalSpawnDelay)
            {
                Transform decal = decalPool.GetPooledObject().transform;
                decal.position = hit.point;
                decal.forward = -hit.normal;
                decalTimer = 0f;
            }

        }

        if(overcharge <= 0)
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
        firehose.overcharge = overcharge;

        if (firehose != null)
        {
            //tween 
        /*
         * Removed due to bug -James
            if (activeTween != null) activeTween.Complete();
            activeTween = firehose.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
        */
            //vfx start
            firehose.VFX_Flame.Play();

            //audio start

            if(audioProfile.activeSource.volume < 0.01f)
                AudioPlayer.PlaySFX(audioProfile.flamethrowerStart, transform);

            if (audioProfile.activeFade != null) 
                StopCoroutine(audioProfile.activeFade);

            audioProfile.activeFade = StartCoroutine(AudioPlayer.FadeAudioIn(audioProfile.activeSource, audioProfile.fadeDuration));
        }
    }

    // Tell everyone to stop burning shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_StopFire(NetworkId _hoseId)
    {
        Flamethrower firehose = Runner.FindObject(_hoseId).GetComponent<Flamethrower>();
        firehose.isShooting = false;
        firehose.overcharge = overcharge;

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
        overcharge = isShooting ? overcharge + Time.fixedDeltaTime * overchargeGainRate : overcharge - Time.fixedDeltaTime * overchargeCoolRate;
        overcharge = Mathf.Clamp(overcharge, 0, maxOvercharge);

        if (overcharge > 0)
        {
            //lerp indicators
            fuelBarAnchor.localScale = new Vector3(1, Mathf.Lerp(0, 1, overcharge / maxOvercharge), 1);
            fuelBarMesh.material.color = Color.Lerp(Color.green, Color.red, overcharge / maxOvercharge);
            heatApplied = heatAppliedDefault;

            //lerp audio pitch
            audioProfile.activeSource.pitch = Mathf.Lerp(.8f, 1.3f, overcharge / maxOvercharge);

            //lerp camerashake
            if(overcharge/maxOvercharge > 0.75f)
            {
                float shakeMagnitude = Mathf.Lerp(0.001f, 0.05f, Mathf.InverseLerp(0.75f, maxOvercharge, overcharge));
                CinemachineShake.ShakeAllCameras(shakeMagnitude, 0.2f);
            }

            if (overcharge >= maxOvercharge)
            {
                fuelBarMesh.material.color = Color.magenta;
                CinemachineShake.ShakeAllCameras(0.1f, 0.2f);
                heatApplied = heatAppliedMax;
            }
        }
        else //disable when empty
        {
            fuelBarElements.SetActive(false);
        }
    }
}
