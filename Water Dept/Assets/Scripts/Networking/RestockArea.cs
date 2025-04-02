using System.Collections.Generic;
using System.Threading;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RestockArea : NetworkBehaviour
{
    [Tooltip("The spot a drone drops a barrel to")]
    [SerializeField] public Vector3 droneDeliveryPoint;

    private bool canSpawn = true;

    public List<GameObject> barrelList;

    [Tooltip("A drones Spawner (empty gameObject with DroneDeliver script attached)")]
    [SerializeField] private DroneDeliver droneDeliver;


    private void Update()
    {
        // Get this shit outta here - Jerry :3
        //Debug.Log(barrelList.Count);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Barrel")
        {
            if(checkList(other.gameObject) == -1)
            {
                barrelList.Add(other.gameObject);
            }

            canSpawn = true;
        }
    }

    //if theres no barrels in zone, send a drone to said delivery point
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Barrel")
        {
            barrelList.Remove(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isListNull() && canSpawn)
        {
            barrelList.Clear();
            droneDeliver.sendDrone(droneDeliveryPoint);
            canSpawn = false;
        }
    }

    //checks list if a barrel is in it and returns index of barrel
    private int checkList(GameObject barrel)
    {
        foreach(GameObject barr in barrelList)
        {
            if(barr == barrel)
            {
                return barrelList.FindInstanceID(barr);
            }
        }
        return -1;
    }

    //if a barrel isnt in the list
    private bool isListNull()
    {
        foreach(GameObject barr in barrelList)
        {
            if(barr != null)
            {
                return false;
            }
        }
        return true;
    }

}
