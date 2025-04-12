using Fusion;
using Fusion.Addons.Physics;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarrelInfo : NetworkBehaviour
{
    public GameObject tail = null;
    public GameObject head = null;

    [SerializeField] private ParticleSystem pixelBoom;

    [Tooltip("How much firedamage burnables take from explosion")]
    [SerializeField] public int heatDamage = 10;
    public float delay = 0.2f;
    public float chainDelay = 1f;

    public bool isThrown = false;

    [Networked] private NetworkBool isExploded { set; get; }
    private bool isSpawned;
    private bool explodingStarted = false;

    [SerializeField] public float explosionRadius = 3.0f;
    [SerializeField] public float explosionForce = 10.0f;
    [SerializeField] public float wallExplosiveForce = 0.0001f;
    [SerializeField] public float rumbleMultiplier = 2.0f;

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
        if (!explodingStarted)
        {
            explodingStarted = true;
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

        yield return new WaitForSeconds(delay);
        ExplosionTriggers();

        yield return new WaitForSeconds(chainDelay);

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

        Destroy(pixelBoom, 2f);
        Destroy(gameObject);
    }

    //calls explode with delay
    IEnumerator DelayedChainExplode()
    {
        yield return new WaitForSeconds(chainDelay);
        Explode();
    }

    //triggers that play when an object explodes
    private void ExplosionTriggers()
    {
        //play sfx
        AudioPlayer.PlaySFX(explodeSFX, transform.position, 0.1f);

        //play anim
        pixelBoom.Play();
        pixelBoom.transform.parent = null;
        isExploded = false;

        StartCoroutine(RumbleController());

        //dereference components to prevent stack overf;pw
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            //Destroy(gameObject.GetComponent<Rigidbody>());
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

            if(hit.GetComponentInParent<PlayerMovement>())
            {
                hit.GetComponentInParent<PlayerMovement>().ExplosionHit(hit.gameObject.transform.position - transform.position, 
                    Vector3.Distance(hit.gameObject.transform.position, transform.position));
            }
            
        }

        //explode other barrels
        Collider[] colls = Physics.OverlapBox(gameObject.transform.position, new Vector3(explosionRadius, 0.01f, explosionRadius), Quaternion.identity);
        foreach (Collider hit in colls)
        {
            if(hit.GetComponent<Burnable>())
            {
                hit.GetComponent<Burnable>().AddHeatModifier(heatDamage);
            }
            
            BarrelInfo barr = hit.GetComponent<BarrelInfo>();

            //if barrel is horizontal and not itself blow it up after delay
            if (barr != null && barr != gameObject.GetComponent<BarrelInfo>() && !hit.CompareTag("DestructibleObject"))
            {
                barr.isExploded = true;
                barr.DelayedChainExplode();
            }

            if (hit.CompareTag("DestructibleObject"))
            {
                Destroy(hit.gameObject);
            }
        }
        //sets out zone tmp
        transform.position = new Vector3(1000,1000,1000);
    }

    //hell on earth :( (very very very temporary)
    //snaps a thrown barrel to other barrels or ground
    private void ThrowSnapping()
    {
        if (isThrown)
        {
            timer = 0;
            isThrown = false;
            isAirborne = true;
        }
        else if (isAirborne)
        {
            timer += Time.deltaTime;
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
                        if (ray.collider.GetComponent<BarrelInfo>() == null && !hit.gameObject.GetComponent<Collider>().isTrigger)
                        {
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                            
                            float dist = Vector3.Distance(gameObject.transform.position, ray.point);
                            gameObject.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);
                            isAirborne = false;
                            gameObject.GetComponent<Rigidbody>().isKinematic = false;
                            
                        }
                    }
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

        if (other.tag == "Gas Flame") {
            StartCoroutine(DelayedChainExplode());
        }
    }

    private IEnumerator RumbleController()
    {
        Gamepad.current.SetMotorSpeeds(0.25f * rumbleMultiplier, 0.75f * rumbleMultiplier);

        yield return new WaitForSeconds(0.75f);

        Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
    }
}

