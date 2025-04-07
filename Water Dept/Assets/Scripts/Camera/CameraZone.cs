using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : NetworkBehaviour
{
    //[SerializeField] string triggerTag = "Player";
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue;
    CinemachineCamera cam;

    private void Awake()
    {
        cam = GetComponentInChildren<CinemachineCamera>();
        cam.Priority = -1;
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2); //Log base 2 to find player layer because LayerMask is a bitmask ts
    }

    private void OnTriggerEnter(Collider other)
    {
        //PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        //if (player == null) 
        //    return;

        //if(player.HasInputAuthority)
        //{
        //    cam.Priority = 10;
        //}

        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject otherNetworkObj = other.transform.parent.GetComponent<NetworkObject>();

            RPC_TriggerEnterZone(Object.Id, otherNetworkObj.Id);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        //if (player == null)
        //    return;

        //if (player.HasInputAuthority)
        //{
        //    cam.Priority = -1;
        //}

        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject otherNetworkObj = other.transform.parent.GetComponent<NetworkObject>();

            RPC_TriggerExitZone(Object.Id, otherNetworkObj.Id);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_TriggerEnterZone(NetworkId _zoneId, NetworkId _otherId)
    {
        CameraZone objectiveMarker = Runner.FindObject(_zoneId).GetComponent<CameraZone>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);

        // Update cam for just client
        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority && Runner.IsClient)
        {
            objectiveMarker.cam.Priority = 10;
        }
        // Update cam for just host
        else if (objectiveMarker != null && inputAuth != null && inputAuth.HasStateAuthority && inputAuth.HasInputAuthority && !Runner.IsClient)
        {
            objectiveMarker.cam.Priority = 10;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_TriggerExitZone(NetworkId _zoneId, NetworkId _otherId)
    {
        CameraZone objectiveMarker = Runner.FindObject(_zoneId).GetComponent<CameraZone>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);

        // Update cam for just client
        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority && Runner.IsClient)
        {
            objectiveMarker.cam.Priority = -1;
        }
        // Update cam for just host
        else if (objectiveMarker != null && inputAuth != null && inputAuth.HasStateAuthority && inputAuth.HasInputAuthority && !Runner.IsClient)
        {
            objectiveMarker.cam.Priority = -1;
        }
    }
}
