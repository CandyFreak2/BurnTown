using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> pooledObjects;
    public GameObject objectTemplate;
    [HideInInspector] public Transform objectPool;
    public int numberOfObjects;
    public bool useRepoolDelay = true;
    public float repoolDelay = 20.0f;

    private int index = 0;

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        objectPool =  new GameObject("Object Pool").transform;
        objectPool.parent = transform;

        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject obj = Instantiate(objectTemplate);
            obj.SetActive(false);

            PooledObject pObj = obj.GetComponent<PooledObject>();
            if (pObj == null) 
                pObj = obj.AddComponent<PooledObject>();

            pObj._Pool = this;
            pObj.SetPooling(useRepoolDelay, repoolDelay);
            obj.transform.parent = objectPool;
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        if (index >= numberOfObjects)
            index = 0;

        GameObject obj = pooledObjects[index];
        obj.SetActive(true);
        obj.transform.parent = null;

        index++;
        return obj;
    }
}
