using Fusion;
using UnityEngine;

public class BarrelController : NetworkBehaviour
{
    [SerializeField] private NetworkObject barrelPrefab;
    [SerializeField] private NetworkObject barrelSchematicPrefab;

    private bool isSchematicOut = false;
    private Transform orientation;
    private NetworkObject barrelSchematic;

    [Tooltip("Distance barrel is placed from player")]
    [SerializeField] public int placeDistance = 3;

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        if(isSchematicOut)
        {
            SnapPosition(barrelSchematic);
        }
        
    }

    //assigns a barrels position to snap to the ground and/or a barrel nearby
    private void SnapPosition(NetworkObject obj)
    {
        Vector3 barrelPos;

        //get initial position in front of player
        barrelPos = (gameObject.GetComponent<Transform>().transform.position) + orientation.forward * placeDistance;

        obj.transform.position = barrelPos;
    }

    //toggles schematic state (called from PlayerMovement)
    public void ToggleSchematic()
    {
        //if(!Runner.IsClient)
        //{
        //    if (isSchematicOut)
        //    {
        //        Runner.Despawn(barrelSchematic);
        //        isSchematicOut = false;
        //    }
        //    else
        //    {
        //        barrelSchematic = Runner.Spawn(barrelSchematicPrefab);
        //        isSchematicOut = true;
        //    }
        //}
        //else if(Runner.IsClient)
        //{
        //    RPC_ToggleBarrel(Object.Id);
        //}
        RPC_ToggleBarrel(Object.Id);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleBarrel(NetworkId playerScript)
    {
        BarrelController pBarrelController = Runner.FindObject(playerScript).GetComponent<BarrelController>();

        if (pBarrelController.isSchematicOut)
        {
            Runner.Despawn(barrelSchematic);
            pBarrelController.isSchematicOut = false;
        }
        else if(!pBarrelController.isSchematicOut)
        {
            pBarrelController.barrelSchematic = Runner.Spawn(barrelSchematicPrefab);
            pBarrelController.isSchematicOut = true;
        }
    }

    //stores the players orientation (called from PlayerMovement)
    public void SetOrientation(Transform playerOrientation)
    {
        orientation = playerOrientation;
    }
}
