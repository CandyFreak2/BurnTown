using NUnit.Framework.Internal;
using System.Collections;
using System.Threading;
using UnityEngine;

public class BarrelInfo : MonoBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    public float delay = 0.5f;

    public bool isThrown = false;
    [SerializeField] private float throwForce = 450;

    public bool isExploded = false;

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
        }

        //if we want them to stack after throw

        //RaycastHit hit;

        //if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.55f) && timer >= 0.4f)
        //{
        //    if (hit.collider.gameObject.tag == "ground")
        //    {
        //        isThrown = false;
        //    }
        //}
    }

    IEnumerator boom()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        Destroy(gameObject.GetComponent<Rigidbody>());
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

