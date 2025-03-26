using System;
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

    [Header("Audio")]
    public SFXProfile sfxProfile;
    [Serializable]
    public class SFXProfile
    {
        // first player to enter truck plays truck activation sfx, all following players play truck door sfx
        [UnityEngine.Range(0f, 1f)] public float footstepVol;
        public List<AudioClip> TruckActivation = new List<AudioClip>();
        public List<AudioClip> TruckAmbient = new List<AudioClip>();
        public List<AudioClip> TruckAccelerate = new List<AudioClip>();
        public List<AudioClip> TruckFast = new List<AudioClip>();
        public List<AudioClip> TruckDoor = new List<AudioClip>();

    }

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
            AudioPlayer.PlaySFX(sfxProfile.TruckActivation, driverPos);


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
