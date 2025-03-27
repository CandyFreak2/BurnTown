using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public float Opacity { get { return opacity; } set { opacity = Mathf.Clamp01(value); } }
    [SerializeField] private float opacity = 1;
    [SerializeField] List<ParticleSystem> vfx;

    private void Awake()
    {
        vfx = transform.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    private void FixedUpdate()
    {
        foreach (ParticleSystem p in vfx)
        {
            ParticleSystem.MinMaxCurve value = new ParticleSystem.MinMaxCurve();
            value.constant = opacity;
            p.customData.SetVector(ParticleSystemCustomData.Custom1, 0, value);
        }
    }
}
