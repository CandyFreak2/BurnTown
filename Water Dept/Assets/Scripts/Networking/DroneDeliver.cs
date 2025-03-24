using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class DroneDeliver : NetworkBehaviour
{
    [SerializeField] private NetworkObject dronePrefab;

    //sends the drone to the delivery position and passes return position to it
    public void sendDrone(Vector3 deliveryPosition)
    {
        NetworkObject drone = Runner.Spawn(dronePrefab, transform.position);

        drone.GetComponent<Drone>().deliverPosition = deliveryPosition;
        drone.GetComponent<Drone>().returnPosition = transform.position;

        drone.GetComponent<Rigidbody>().AddForce((deliveryPosition - transform.position)* 20);   
    }
}
