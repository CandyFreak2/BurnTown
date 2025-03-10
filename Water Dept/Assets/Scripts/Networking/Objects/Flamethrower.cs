using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Fusion;
using DG.Tweening;
using System;

public class Flamethrower : NetworkBehaviour
{
    [Networked] private NetworkBool isShooting { get; set; }

    /*[Header("Fuel")]
    [SerializeField] GameObject fuelPrefab;
    [SerializeField] float fuelSpawnDelay = 0.2f;
    float fuelTimer;*/

    [Header("Damage & Fire")]
    [SerializeField] int heatApplied = 1;

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

    public override void Spawned()
    {
        isShooting = false;
        audioProfile.activeSource = AudioPlayer.PlaySFXLoop(audioProfile.flamethrowerLoop, transform, 0);
    }

    public void Spray()
    {
        RPC_ShootFire(Object.Id);
        isShooting = true;
    }

    public void StopSpraying()
    {
        RPC_StopFire(Object.Id);
        isShooting = false;
    }

    public override void FixedUpdateNetwork()
    {
        if(!isShooting)
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
    }

    // Tell everyone to burn shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_ShootFire(NetworkId _hoseId)
    {
        Flamethrower firehose = Runner.FindObject(_hoseId).GetComponent<Flamethrower>();

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

        if (firehose != null)
        {
            //vfx stop
            firehose.VFX_Flame.Stop();

            //audio stop
            if (audioProfile.activeFade != null) StopCoroutine(audioProfile.activeFade);
            audioProfile.activeFade = StartCoroutine(AudioPlayer.FadeAudioOut(audioProfile.activeSource, audioProfile.fadeDuration));
        }
    }
}
