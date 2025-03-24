using Fusion;
using UnityEngine;

public class BarrelController : NetworkBehaviour
{
    [SerializeField] private NetworkObject barrelPrefab;
    [SerializeField] private NetworkObject barrelSchematicPrefab;

    [Networked] private bool isSchematicOut { get; set; }
    [Networked] private Vector3 orientationForward { get; set; }
    [Networked] private NetworkObject barrelSchematic { get; set; }

    [Tooltip("Distance barrel is placed from player")]
    [SerializeField] public int placeDistance = 3;

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
        //get initial position in front of player
        obj.transform.position = (gameObject.GetComponent<Transform>().transform.position) + orientationForward * placeDistance;

        RaycastHit hit;

        if (Physics.Raycast(obj.transform.position + new Vector3(0, 100f, 0), Vector3.down, out hit))
        {
            //snaps barrel to ground
            if (hit.collider.GetComponent<BarrelInfo>() == null)
            {
                float dist = Vector3.Distance(obj.transform.position, hit.point);
                obj.transform.position -= Vector3.up * dist - new Vector3(0, 0.7f, 0);
            }
            //if its a barrel, snaps position to on top of the barrel
            else if (hit.collider.GetComponent<BarrelInfo>() != null)
            {
                obj.transform.position = hit.collider.transform.position + new Vector3(0, 1.2f, 0);

                //links head/tail chain components of barrelInfo on place
                if (obj.GetComponent<BarrelInfo>() != null)
                {
                    obj.GetComponent<BarrelInfo>().tail = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<BarrelInfo>().head = obj.gameObject;
                }
            }
        }
    }

    //stores the players orientation (called from PlayerMovement)
    public void SetOrientation(Vector3 playerOrientationF)
    {
        orientationForward = playerOrientationF;
    }

    //toggles schematic state (called from PlayerMovement)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleBarrel(NetworkId playerScript)
    {
        BarrelController pBarrelController = Runner.FindObject(playerScript).GetComponent<BarrelController>();

        if (pBarrelController.isSchematicOut)
        {
            Runner.Despawn(barrelSchematic);
            pBarrelController.isSchematicOut = false;
        }
        else if (!pBarrelController.isSchematicOut)
        {
            pBarrelController.barrelSchematic = Runner.Spawn(barrelSchematicPrefab);
            pBarrelController.isSchematicOut = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlaceBarrel(NetworkId playerScript)
    {
        RPC_ToggleBarrel(playerScript);

        BarrelController pBarrelController = Runner.FindObject(playerScript).GetComponent<BarrelController>();

        pBarrelController.barrelSchematic = Runner.Spawn(barrelPrefab);
        pBarrelController.SnapPosition(barrelSchematic);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ThrowBarrel(NetworkId playerScript)
    {
        RPC_ToggleBarrel(playerScript);
        
        BarrelController pBarrelController = Runner.FindObject(playerScript).GetComponent<BarrelController>();

        pBarrelController.barrelSchematic = Runner.Spawn(barrelPrefab);
        pBarrelController.SnapPosition(barrelSchematic);

        pBarrelController.barrelSchematic.GetComponent<BarrelInfo>().isThrown = true;
        pBarrelController.barrelSchematic.GetComponent<Rigidbody>().AddForce((orientationForward + new Vector3(0, 1, 0)) * 500);

    }
}
