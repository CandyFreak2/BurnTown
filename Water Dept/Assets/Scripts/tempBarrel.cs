using UnityEngine;

public class tempBarrel : MonoBehaviour
{
    [SerializeField] private GameObject barrelPrefab;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            placeBarrel();
        }
    }

    void placeBarrel()
    {
        barrelPrefab.transform.position = (gameObject.gameObject.GetComponent<Transform>().transform.position) + gameObject.GetComponent<Transform>().transform.forward * 10;
    }

}
