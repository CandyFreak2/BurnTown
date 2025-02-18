using NUnit.Framework.Internal;
using System.Collections;
using System.Data;
using System.Linq;
using System.Threading;
using UnityEngine;

public class BarrelInfo : MonoBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    [SerializeField] private ParticleSystem pixelBoom;

    public float delay = 0.5f;

    public bool isThrown = false;
    
    [SerializeField] private float throwForce = 450;

    [SerializeField] public float explosionRadius = 5.0f;
    [SerializeField] public float explosionForce = 10.0f;
    public bool isExploded = false;

    private bool isAirborne;
    private float timer;

    public void Explode()
    {
        StartCoroutine(ChainExplode());
    }

    private void Update()
    {
        ThrowBarrel();

        if(isExploded)
        {
            StartCoroutine(ChainExplode());
        }
    }

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
            //if we want them to stack after throw

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

    IEnumerator ChainExplode()
    {
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

        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    private void ExplosionTriggers()
    {
        pixelBoom.Play();

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
        }
        Destroy(gameObject.GetComponent<MeshRenderer>());

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, gameObject.transform.position, explosionRadius, 3.0f);
                Debug.Log("push");
            }
        }
    }
}

