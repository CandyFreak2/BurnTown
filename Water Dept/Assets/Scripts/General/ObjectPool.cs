using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> pooledObjects;
    public GameObject objectTemplate;
    public int numberOfObjects;
    public bool useRepoolDelay = true;
    public float repoolDelay = 20.0f;

    private int index = 0;

    private void Start()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject obj = Instantiate(objectTemplate);
            obj.SetActive(false);
            obj.AddComponent<PooledObject>();
            obj.GetComponent<PooledObject>()._Pool = this;
            obj.transform.parent = transform;
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        if (index > numberOfObjects)
            index = 0;

        GameObject obj = pooledObjects[index];
        obj.SetActive(true);

        index++;
        return obj;
    }
}
