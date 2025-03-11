using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CarData : NetworkBehaviour
{
    [SerializeField] public Rigidbody carRB;
    [SerializeField] public List<Tire> frontTires, backTires;

    [SerializeField] public Transform driverPos;
    [SerializeField] public List<Transform> passengerPos;
    [Networked] private int numPassengers { set; get; }

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
    }

    public void AddPassenger()
    {
        numPassengers++;
    }

    public void SubPassenger()
    {
        numPassengers--;
    }

    public int GetNumPassengers()
    {
        return numPassengers;
    }
}
