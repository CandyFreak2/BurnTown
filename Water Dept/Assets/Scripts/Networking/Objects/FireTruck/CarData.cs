using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CarData : NetworkBehaviour
{
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private List<Tire> frontTires, backTires;

    [SerializeField] private Transform driverPos;
    [SerializeField] private List<Transform> passengerPos;
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
}
