using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RestockArea : NetworkBehaviour
{
    [Tooltip("The spot a drone drops a barrel to")]
    [SerializeField] public Vector3 droneDeliveryPoint;

    private int barrelCount;
    private bool canSpawn = true;

    [Tooltip("A drones Spawner (empty gameObject with DroneDeliver script attached)")]
    [SerializeField] private DroneDeliver droneDeliver;

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Barrel")
        {
            canSpawn = true;
            barrelCount++;
        }
    }

    //if theres no barrels in zone, send a drone to said delivery point
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Barrel")
        {
            barrelCount--;

            if(barrelCount == 0 && canSpawn)
            {
                droneDeliver.sendDrone(droneDeliveryPoint);
                canSpawn = false;
            }
        }
    }

}
