using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class tempBarrel : MonoBehaviour
{
    [SerializeField] private GameObject placeholderPrefab;
    [SerializeField] private GameObject barrelPrefab;

    private bool isSchemOut = false;
    private bool isThrowing = false;
    private GameObject schematic;

    [Tooltip("Distance barrel is placed from player")]
    [SerializeField] public int placeDistance = 3;
    [SerializeField] public int pickupDistance = 2;

    private Transform orientation;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isSchemOut)
        {
            PlaceBarrel();
            if (isSchemOut)
            {
                isSchemOut = false;
                Destroy(schematic);
            }
        }


        if (isSchemOut)
        {
            SchemBarrel();
        }
    }

    //queue schematic if barrel is nearby
    public void BarrelSchematics(Transform playerOrientation)
    {
        if (!isSchemOut)
        {
            isSchemOut = true;
            schematic = Instantiate(placeholderPrefab);

            orientation = playerOrientation;
        }
    }

    public void BarrelThrow(Transform playerOrientation)
    {
        if(isSchemOut)
        {
            isThrowing = true;

            GameObject bar = Instantiate(barrelPrefab);
            bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) + playerOrientation.forward * placeDistance;
            bar.GetComponent<BarrelInfo>().isThrown = true;
            bar.AddComponent<Rigidbody>();
            bar.GetComponent<Rigidbody>().AddForce((orientation.forward + new Vector3(0, 1, 0)) *  1000);

            SnapPosition(bar);

            if (isSchemOut)
            {
                isSchemOut = false;
                Destroy(schematic);
            }

            //prevents player from throwign on stacks
            isThrowing = false;
        }
        
    }

    //gets repalced by existing pickup
    //private bool PickupBarrel()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, pickupDistance);
    //    foreach (Collider hit in colliders)
    //    {
    //        if(hit.tag == "Barrel")
    //        {
    //            Destroy(hit.gameObject);
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //hovers a schematic prefab in front of the player indicating where a barrel will be placed
    private void SchemBarrel()
    {
        isSchemOut = true;
        if (isSchemOut)
        {
            schematic.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
               orientation.forward * placeDistance;
        }

        SnapPosition(schematic);
    }

    //places a barrel prefab where the schematic was previously
    private void PlaceBarrel()
    {
        GameObject bar = Instantiate(barrelPrefab);
        bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
                gameObject.GetComponent<Transform>().transform.forward * placeDistance;

        SnapPosition(bar);
    }

    //assigns a barrels position to snap to the ground and/or a barrel nearby
    private void SnapPosition(GameObject obj)
    {
        RaycastHit hit;
        //casts ray from the sky down
        if (Physics.Raycast(obj.transform.position + new Vector3(0, 100f, 0), Vector3.down, out hit))
        {
            //snaps barrel to ground
            if (hit.collider.GetComponent<BarrelInfo>() == null)
            {
                float dist = Vector3.Distance(obj.transform.position, hit.point);
                obj.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);
            }
            //if its a barrel, snaps position to on top of the barrel
            else if (hit.collider.GetComponent<BarrelInfo>() != null && !isThrowing)
            {
                obj.transform.position = hit.collider.transform.position + new Vector3(0, 1.0f, 0);

                //links head/tail chain components of barrelInfo on place
                if (obj.GetComponent<BarrelInfo>() != null)
                {
                    obj.GetComponent<BarrelInfo>().tail = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<BarrelInfo>().head = obj.gameObject;
                }
            }
        }
    }

}
