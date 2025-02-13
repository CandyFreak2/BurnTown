using UnityEngine;
using System.Collections;
using Fusion;

public class FireHouse : NetworkBehaviour
{
    [SerializeField] GameObject water;
    [SerializeField] public float waterForce = 10f;

    [SerializeField] private ParticleSystem flamethrower;
    public void Spray()
    {
        water.SetActive(true);
        flamethrower.Play();
    }

    public IEnumerator StopSpraying()
    {
        yield return new WaitForSeconds(0.5f);
        water.SetActive(false);
        flamethrower.Stop();
    }

    public void DropHose()
    {
        gameObject.tag = "Untagged";

        Invoke("ReactivateHose", 1.0f);
    }

    private void ReactivateHose()
    {
        gameObject.tag = "FireHoseHead";
    }
}
