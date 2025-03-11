using Fusion;
using System.Collections;
using UnityEngine;

public class BarrelInfo : NetworkBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    [SerializeField] private ParticleSystem pixelBoom;

    public float delay = 0.5f;

    public bool isThrown = false;

    [Networked] private NetworkBool isExploded { set; get; }
    private bool isSpawned;

    [SerializeField] public float explosionRadius = 3.0f;
    [SerializeField] public float explosionForce = 10.0f;
    [SerializeField] public float wallExplosiveForce = 0.0001f;

    //SFX
    [Header("SFX")]
    [SerializeField] public AudioClip[] pickupSFX;
    [SerializeField] public AudioClip[] placeSFX;
    [SerializeField] public AudioClip[] stackSFX;
    [SerializeField] public AudioClip[] throwSFX;
    [SerializeField] public AudioClip[] impactSFX;
    [SerializeField] public AudioClip[] explodeSFX;

    [Range(0,3)]
    [SerializeField] private float throwSnapRadius = 2.0f;
    public bool isAirborne;
    private float timer;

    public override void Spawned()
    {
        isExploded = false;
        isSpawned = true;
    }

    public override void FixedUpdateNetwork()
    {
        ThrowBarrel();
        if (isSpawned)
        {
            if (isExploded)
            {
                StartCoroutine(ChainExplode());
            }
        }
    }

    public void Explode()
    {
        if (!isExploded)
        {
            StartCoroutine(ChainExplode());
        }
    }


    //Throws barrel with throwForce and explodes when it hits ground
    public void ThrowBarrel()
    {
        if (isSpawned)
        {
            ThrowSnapping();
        }
    }

    //explodes current barrel and then linked barrels with delay
    IEnumerator ChainExplode()
    {
        if (head != null)
        {
            head.GetComponent<Rigidbody>().isKinematic = true;
        }

        if (tail != null)
        {
            tail.GetComponent<Rigidbody>().isKinematic = true;
        }

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
        //play sfx
        AudioPlayer.PlaySFX(explodeSFX, transform.position, 0.1f);

        //play anim
        pixelBoom.Play();
        isExploded = false;

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
                if (rb.gameObject.CompareTag("DestructibleObject"))
                {
                    rb.gameObject.transform.GetComponent<ConcreteWall>().RPC_Explode();
                }
                rb.AddExplosionForce(explosionForce, gameObject.transform.position, explosionRadius, 3.0f);
            }

        }

        //explode other barrels
        Collider[] colls = Physics.OverlapBox(gameObject.transform.position, new Vector3(explosionRadius, 0.01f, explosionRadius), Quaternion.identity);
        foreach (Collider hit in colls)
        {
            BarrelInfo barr = hit.GetComponent<BarrelInfo>();

            //if barrel is horizontal and not itself blow it up after delay
            if (barr != null && barr != gameObject.GetComponent<BarrelInfo>() && !hit.CompareTag("DestructibleObject"))
            {
                StartCoroutine(DelayedChainExplode(barr));
            }

            if (hit.CompareTag("DestructibleObject"))
            {
                Destroy(hit.gameObject);
            }
        }
    }

    //hell on earth :( (very very very temporary)
    //snaps a thrown barrel to other barrels or ground
    private void ThrowSnapping()
    {
        if (isThrown)
        {
            isThrown = false;
            isAirborne = true;
        }
        else if (isAirborne)
        {
            timer += Time.deltaTime;
            //if collides with something mid air, blow up
            Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position - new Vector3(0, 0.5f, 0), throwSnapRadius);
            foreach (Collider hit in colliders)
            {
                if (timer > 0.2f && hit.GetComponent<BarrelInfo>() && hit.GetComponent<BarrelInfo>() != gameObject.GetComponent<BarrelInfo>() && isAirborne)
                {
                    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    

                    transform.position = hit.transform.position + new Vector3(0, 1, 0);

                    isAirborne = false;
                    // links head/ tail chain components of barrelInfo on place
                    if (gameObject.GetComponent<BarrelInfo>() != null)
                    {
                        tail = hit.gameObject;
                        hit.gameObject.GetComponent<BarrelInfo>().head = gameObject;
                    }
                    gameObject.GetComponent<Rigidbody>().isKinematic = false;
                    Debug.Log("SnapBox");
                }
            }
            Collider[] colliders2 = Physics.OverlapSphere(gameObject.transform.position - new Vector3(0, 0.5f, 0), 0.5f);
            foreach (Collider hit in colliders2)
            {
                if (hit != gameObject.GetComponent<Collider>() && timer > 0.2f)
                {
                    RaycastHit ray;
                    //casts ray from the sky down
                    if (Physics.Raycast(gameObject.transform.position, Vector3.down * 100, out ray))
                    {
                        //snaps barrel to ground
                        if (ray.collider.GetComponent<BarrelInfo>() == null)
                        {
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                            gameObject.GetComponent<Rigidbody>().isKinematic = false;

                            float dist = Vector3.Distance(gameObject.transform.position, ray.point);
                            gameObject.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);
                            isAirborne = false;
                        }
                    }
                    Debug.Log("SnapGround");
                }

            }


        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Flame")
        {
            isExploded = true;
        }
    }
}

