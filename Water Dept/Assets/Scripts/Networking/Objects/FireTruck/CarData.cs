using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class CarData : NetworkBehaviour
{
    [SerializeField] public Rigidbody carRB;
    [SerializeField] public Collider killzone;
    [SerializeField] public List<Tire> frontTires, backTires;

    [SerializeField] public Transform driverPos;
    [SerializeField] public List<Transform> passengerPos;
    [Networked] private int numPassengers { set; get; } // change to private
    private int maxPassengers;
    [Networked] public Vector3 truckPos { get; set; }
    [Networked] private float carSpeed { get; set; }
    [SerializeField] public float killSpeed = 4.0f;
    private bool atKillSpeed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            truckPos = transform.position;
            UpdateKillzone();
            //killzone.transform.localScale = new Vector3(1.0f, 1.0f, Mathf.Clamp(carSpeed / 4, 1.0f, 3.75f));
        }
    }

    private void UpdateKillzone()
    {
        carSpeed = carRB.linearVelocity.magnitude;

        if (!atKillSpeed && carSpeed >= 5.0f)
        {
            killzone.enabled = true;
            atKillSpeed = true;
        }
        else if (atKillSpeed && carSpeed < 5.0f) // change back
        {
            killzone.enabled = false;
            atKillSpeed = false;
        }
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
