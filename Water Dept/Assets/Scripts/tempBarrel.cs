using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class tempBarrel : MonoBehaviour
{
    [SerializeField] private GameObject placeholderPrefab;
    [SerializeField] private GameObject barrelPrefab;

    private bool isSchemOut = false;
    private GameObject schematic;

    public const int PLACE_DISTANCE = 3;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(isSchemOut)
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

        if(Input.GetKeyDown(KeyCode.E) && isSchemOut)
        {
            placeBarrel();
        }

        if(isSchemOut)
        {
            schemBarrel();
        }
    }

    private void schemBarrel()
    {
        isSchemOut = true;
        if(isSchemOut)
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

    private void snapPosition(GameObject obj)
    {
        RaycastHit hit;

        if (Physics.Raycast(obj.transform.position + new Vector3(0, 100f, 0), Vector3.down, out hit))
        {
            float dist = Vector3.Distance(obj.transform.position, hit.point);
            obj.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);

        }
    }

}
