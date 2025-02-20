using UnityEngine;
using System.Collections;
using Fusion;

public class FireHouse : NetworkBehaviour
{
    [SerializeField] GameObject flame;
    [Networked] private NetworkBool isShooting { get; set; }
    [SerializeField] public float waterForce = 10f;

    [SerializeField] private ParticleSystem flamethrower;

    public override void Spawned()
    {
        isShooting = false;
    }

    public void Spray()
    {
        flame.SetActive(true);
        RPC_ShootFire(Object.Id);
        isShooting = true;
    }

    public IEnumerator StopSpraying()
    {
        yield return new WaitForSeconds(0.5f);
        flame.SetActive(false);
        RPC_StopFire(Object.Id);
        isShooting = false;
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

    // Tell everyone to burn shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_ShootFire(NetworkId _hoseId)
    {
        FireHouse firehose = Runner.FindObject(_hoseId).GetComponent<FireHouse>();

        if (firehose != null)
        {
            firehose.flamethrower.Play();
        }
    }

    // Tell everyone to stop burning shit
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_StopFire(NetworkId _hoseId)
    {
        FireHouse firehose = Runner.FindObject(_hoseId).GetComponent<FireHouse>();

        if (firehose != null)
        {
            firehose.flamethrower.Stop();
        }
    }
}
