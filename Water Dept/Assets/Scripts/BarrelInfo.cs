using NUnit.Framework.Internal;
using System.Collections;
using System.Data;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class BarrelInfo : MonoBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    [SerializeField] private ParticleSystem pixelBoom;

    public float delay = 0.5f;

    public bool isThrown = false;
    
    [SerializeField] private float throwForce = 450;

    [SerializeField] public float explosionRadius = 3.0f;
    [SerializeField] public float explosionForce = 10.0f;
    public bool isExploded = false;

    private bool isAirborne;
    private float timer;

    private void Update()
    {
        ThrowBarrel();

        if(isExploded)
        {
            StartCoroutine(ChainExplode());
        }
    }

    public void Explode()
    {
        if(!isExploded)
        {
            StartCoroutine(ChainExplode());
        }
    }

    //Throws barrel with throwForce and explodes when it hits ground
    private void ThrowBarrel()
    {
        if(isThrown)
        {
            gameObject.AddComponent<Rigidbody>();
            gameObject.GetComponent<Rigidbody>().AddForce((transform.forward + new Vector3(0,1,0)) * throwForce);
            isThrown = false;
            isAirborne = true;
        }
        if(isAirborne)
        {
            timer += Time.deltaTime;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.55f) && timer > 0.5f)
            {
                if (hit.collider.gameObject.tag == "ground")
                {
                    Explode();
                }
            }
        }

    }

    //explodes current barrel and then linked barrels with delay
    IEnumerator ChainExplode()
    {
        isExploded = false;

        ExplosionTriggers();

        yield return new WaitForSeconds(delay);

        if (head != null)
        {
            head.GetComponent<BarrelInfo>().tail = null;
            head.GetComponent<BarrelInfo>().Explode();
        }
        if (tail != null)
        {
            tail.GetComponent<BarrelInfo>().head = null;
            tail.GetComponent<BarrelInfo>().Explode();
        }

        //pixelBoom.transform.parent = null;

        yield return new WaitForSeconds(delay);

        Destroy(pixelBoom, pixelBoom.main.duration);
        Destroy(gameObject);
    }

    //calls explode with delay
    IEnumerator DelayedChainExplode(BarrelInfo barrel)
    {
        yield return new WaitForSeconds(delay);
        barrel.Explode();
    }

    //triggers that play when an object explodes
    private void ExplosionTriggers()
    {
        //play anim
        pixelBoom.Play();
        Debug.Log("Particle Spawned");

        //dereference components to prevent stack overf;pw
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
        }
        Destroy(gameObject.GetComponent<MeshRenderer>());
        Destroy(gameObject.GetComponent<BoxCollider>());

        //explosion forces
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, gameObject.transform.position, explosionRadius, 3.0f);
            }
        }

        //explode other barrels
        Collider[] colls = Physics.OverlapBox(gameObject.transform.position, new Vector3(explosionRadius, 0.01f, explosionRadius), Quaternion.identity);
        foreach (Collider hit in colls)
        {
            BarrelInfo barr = hit.GetComponent<BarrelInfo>();

            //if barrel is horizontal and not itself blow it up after delay
            if (barr != null && barr != gameObject.GetComponent<BarrelInfo>())
            {
                StartCoroutine(DelayedChainExplode(barr));
            }
        }
    }
}

