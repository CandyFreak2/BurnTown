using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditor.SearchService;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class tempBarrel : MonoBehaviour
{
    [SerializeField] private GameObject placeholderPrefab;
    [SerializeField] private GameObject barrelPrefab;

    private bool isSchemOut = false;
    private GameObject schematic;

    public const int PLACE_DISTANCE = 3;

    public GameObject testBarrel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isSchemOut)
            {
                isSchemOut = false;
                Destroy(schematic);
            }
            else
            {
                isSchemOut = true;
                schematic = Instantiate(placeholderPrefab);
            }

        }

        if (Input.GetKeyDown(KeyCode.E) && isSchemOut)
        {
            placeBarrel();
        }

        if (Input.GetKeyDown(KeyCode.F) && isSchemOut)
        {
            GameObject bar = Instantiate(barrelPrefab);
            bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
                gameObject.GetComponent<Transform>().transform.forward * PLACE_DISTANCE;
            snapPosition(bar);
            bar.GetComponent<BarrelInfo>().isThrown = true;
        }

        if (isSchemOut)
        {
            schemBarrel();
        }
    }

    private void schemBarrel()
    {
        isSchemOut = true;
        if (isSchemOut)
        {
            schematic.transform.position = (gameObject.GetComponent<Transform>().transform.position) + 
                gameObject.GetComponent<Transform>().transform.forward * PLACE_DISTANCE;
        }

        snapPosition(schematic);

    }

    private void placeBarrel()
    {
        GameObject bar = Instantiate(barrelPrefab);
        bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
                gameObject.GetComponent<Transform>().transform.forward * PLACE_DISTANCE;

        snapPosition(bar);
    }

    //assigns a barrels position to snap to the ground and/or a barrel nearby
    private void snapPosition(GameObject obj)
    {
        RaycastHit hit;

        if (Physics.Raycast(obj.transform.position + new Vector3(0, 100f, 0), Vector3.down, out hit))
        {
            if (hit.collider.GetComponent<BarrelInfo>() == null)
            {
                float dist = Vector3.Distance(obj.transform.position, hit.point);
                obj.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);
            }
            else if(hit.collider.GetComponent<BarrelInfo>() != null)
            {
                obj.transform.position = hit.collider.transform.position + new Vector3(0, 1.0f, 0);

                if(obj.GetComponent<BarrelInfo>() != null)
                {
                    obj.GetComponent<BarrelInfo>().tail = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<BarrelInfo>().head = obj.gameObject;
                }
            }
        }
    }

}
