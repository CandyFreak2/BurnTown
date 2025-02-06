using Unity.VisualScripting;
using UnityEngine;

public class TestPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; 
    public GameObject hand;
    GameObject fireHose;
    public bool isHolding = false;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        
        float horizontal = Input.GetAxis("Horizontal"); 
        float vertical = Input.GetAxis("Vertical"); 

        
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;

        
        transform.Translate(movement);

        if (isHolding)
        {
            LockHosePosition();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && isHolding == true)
        {
            //sprays water
            fireHose.GetComponent<FireHouse>().SprayWater();
            //apply backward force
            WaterForce();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && isHolding == true)
        {
            //stops spraying water after half a second
            fireHose.GetComponent<FireHouse>().StartCoroutine(fireHose.GetComponent<FireHouse>().StopSprayingWater());
        }

        if (Input.GetKeyDown(KeyCode.E) && isHolding)
        {
            Debug.Log("Dropped Hose");
            if (fireHose != null)
            {
                fireHose.transform.SetParent(null, true);
                fireHose.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                fireHose = null; 
                isHolding = false; 
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        
        if (other.gameObject.tag == "FireHoseHead" && !isHolding)
        {
            fireHose = other.gameObject;
            
                Debug.Log("Grabbed Hose");
                isHolding = true;
            fireHose.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                fireHose.transform.position = hand.transform.position; 
                fireHose.transform.SetParent(hand.transform, true); 
            
        }
    }

    void LockHosePosition()
    {
        fireHose.transform.position = hand.transform.position;
    }

    void WaterForce()
    {
        rb.AddForce(-transform.forward * fireHose.GetComponent<FireHouse>().waterForce, ForceMode.Impulse);
    }
}
