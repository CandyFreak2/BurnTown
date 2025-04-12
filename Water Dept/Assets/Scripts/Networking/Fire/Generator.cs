using UnityEngine;
using Fusion;
using Unity.Cinemachine;

public class Generator : NetworkBehaviour
{
    [SerializeField] private Objective PoweredObjective;
    [SerializeField] private Objective PoweredObjective_2;
    [SerializeField] private Objective PoweredObjective_3;
    [SerializeField] private CinemachineCamera gateCamera;
    [SerializeField] private GameObject areaCameraZone;
    [SerializeField] private Burnable genBurnable;
    [SerializeField] private float disabledPercent01; // value 0-1

    [Networked] public NetworkBool isDisabled {  get; set; }

    public override void Spawned()
    {
        PoweredObjective.AddGenerator(this);
        PoweredObjective_2.AddGenerator(this);
        PoweredObjective_3.AddGenerator(this);
    }

    public override void FixedUpdateNetwork()
    {
        bool lastState = isDisabled;
        if (!isDisabled) {
            isDisabled = genBurnable.GetDisabled(disabledPercent01);
        }

        // When isDisabled switches update objective
        if (lastState != isDisabled)
        {
            RPC_ChangeCameraZones(Object.Id);
            PoweredObjective.CheckAllDeactivated();
            PoweredObjective_2.CheckAllDeactivated();
            PoweredObjective_3.CheckAllDeactivated();
        }
        
    }

    private void ResetCamZones()
    {
        //areaCameraZone.SetActive(true);
        gateCamera.Priority = -1;
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_ChangeCameraZones(NetworkId _genId)
    {
        Generator generator = Runner.FindObject(_genId).GetComponent<Generator>();

        if (generator != null && generator.areaCameraZone != null && generator.gateCamera != null)
        {
            generator.areaCameraZone.SetActive(false);
            generator.gateCamera.Priority = 10;
            generator.Invoke("ResetCamZones", 4.0f);
        }
    }
}
