using NUnit.Framework.Internal;
using System.Collections;
using System.Threading;
using UnityEngine;

public class BarrelInfo : MonoBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    [SerializeField] private ParticleSystem pixelBoom;

    public float delay = 0.5f;

    public bool isThrown = false;
    private bool isAirborne;
    [SerializeField] private float throwForce = 450;

    public bool isExploded = false;

    float timer;

    public void explode()
    {
        StartCoroutine(boom());
    }

    private void Update()
    {
        throwBarrel();

        if(isExploded)
        {
            StartCoroutine(boom());
        }
    }

    private void throwBarrel()
    {
        if(isThrown)
        {
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
                    explode();
                }
            }
        }

    }

    IEnumerator boom()
    {
        pixelBoom.Play();
        Destroy(gameObject.GetComponent<Rigidbody>());
        Destroy(gameObject.GetComponent<MeshRenderer>());
        //play explosion
        yield return new WaitForSeconds(delay);
        if (head != null)
        {
            head.GetComponent<BarrelInfo>().tail = null;
            head.GetComponent<BarrelInfo>().explode();
        }
        if (tail != null)
        {
            tail.GetComponent<BarrelInfo>().head = null;
            tail.GetComponent<BarrelInfo>().explode();
        }
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

}

