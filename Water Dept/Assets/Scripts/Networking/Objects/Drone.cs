using System.Collections;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class Drone : NetworkBehaviour
{
    
    public Vector3 deliverPosition;
    public Vector3 returnPosition;

    private bool isReturning = false;
    [SerializeField] private NetworkObject barrel;
 
    private void FixedUpdate()
    {
        if (Vector3.Distance(deliverPosition, transform.position) < 0.5f && !isReturning)
        {
            StartCoroutine(SpawnBarrel(1f));
            isReturning = true;
        }
        else if(Vector3.Distance(returnPosition, transform.position) < 0.5f && isReturning)
        {
            isReturning = false;
            Runner.Despawn(Object);
        }
    }

    IEnumerator SpawnBarrel(float delay)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(delay);

        Runner.Spawn(barrel, transform.position);
        yield return new WaitForSeconds(delay);

        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().AddForce((returnPosition - transform.position) * 20);
    }
}
