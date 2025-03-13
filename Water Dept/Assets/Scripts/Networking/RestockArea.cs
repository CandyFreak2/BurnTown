using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RestockArea : NetworkBehaviour
{
    [Tooltip("The spot a drone drops a barrel to")]
    [SerializeField] public Vector3 droneDeliveryPoint;

    private int barrelCount;

    [SerializeField] private DroneDeliver droneDeliver;

    private void Update()
    {
        Debug.Log(barrelCount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Barrel")
        {
            barrelCount++;
        }
    }

    //if theres no barrels in zone, send a drone to said delivery point
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Barrel")
        {
            barrelCount--;

            if(barrelCount == 0)
            {
                droneDeliver.sendDrone(droneDeliveryPoint);
            }
        }
    }
}
