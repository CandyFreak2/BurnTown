using UnityEngine;
using System.Collections.Generic;

public class SImpleLinkVFXToSpeed : MonoBehaviour
{
    public Rigidbody speedLink;
    public float multiplier = 1;
    public List<ParticleSystem> vfxEmitters;

    private void FixedUpdate()
    {
        if (speedLink != null)
        {
            foreach (ParticleSystem p in vfxEmitters)
            {
                ParticleSystem.EmissionModule rate = p.emission;
                rate.rateOverTime = speedLink.linearVelocity.magnitude * multiplier;
            }
        }
    }
}
