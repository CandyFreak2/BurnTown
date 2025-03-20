using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CarData : NetworkBehaviour
{
    [SerializeField] public Rigidbody carRB;
    [SerializeField] public List<Tire> frontTires, backTires;

    [SerializeField] public Transform driverPos;
    [SerializeField] public List<Transform> passengerPos;
    [Networked] public int numPassengers { set; get; } // change to private
    private int maxPassengers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        numPassengers = 0;
        maxPassengers = passengerPos.Count + 1;
    }

    public void AddPassenger()
    {
        RPC_AddPassenger(Object.Id);
    }

    public void SubPassenger()
    {
        RPC_SubPassenger(Object.Id);
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_AddPassenger(NetworkId carDataId)
    {
        CarData carData = Runner.FindObject(carDataId).GetComponent<CarData>();

        if (carData != null)
        {
            carData.numPassengers++;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by everyone
    public void RPC_SubPassenger(NetworkId carDataId)
    {
        CarData carData = Runner.FindObject(carDataId).GetComponent<CarData>();

        if (carData != null)
        {
            carData.numPassengers--;
        }
    }

    public int GetNumPassengers()
    {
        return numPassengers;
    }

    public int GetMaxPassengers()
    {
        return maxPassengers;
    }
}
