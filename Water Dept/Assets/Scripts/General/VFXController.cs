using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public float Opacity { get { return opacity; } set { opacity = Mathf.Clamp01(value); } }
    [SerializeField] private float opacity = 1;

    [SerializeField] List<ParticleSystem> crawlingVFX;
    [SerializeField] List<ParticleSystem> emitterVFX;
    List<float> emitterRate = new();
    //ParticleSystem.EmissionModule value;

    private void Start()
    {
        //vfx = transform.GetComponentsInChildren<ParticleSystem>().ToList();
        foreach (var emitter in emitterVFX)
        {
            emitterRate.Add(emitter.emission.rateOverTimeMultiplier);
            emitter.Play();
        }
    }

    private void FixedUpdate()
    {
        foreach (ParticleSystem p in crawlingVFX)
        {
            ParticleSystem.MinMaxCurve value = new ParticleSystem.MinMaxCurve();
            value.constant = opacity;
            p.customData.SetVector(ParticleSystemCustomData.Custom1, 0, value);
        }

        for(int i = 0; i < emitterVFX.Count; i++)
        {
            ParticleSystem.EmissionModule value = emitterVFX[i].emission;
            value.rateOverTimeMultiplier = emitterRate[i] * opacity;
        }
    }
}
