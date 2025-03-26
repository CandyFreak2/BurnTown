using Fusion;
using UnityEngine;

public class ObjectiveMarker : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue;
    public bool isTriggered { get; private set; }

    private void Start()
    {
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2); //Log base 2 to find player layer because LayerMask is a bitmask
    }

    public override void Spawned()
    {
        isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject otherNetworkObj = other.transform.parent.GetComponent<NetworkObject>();
            
            RPC_TriggerMarker( Object.Id, otherNetworkObj.Id);
        }
    }

    //[Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_TriggerMarker(NetworkId _markerId, NetworkId _otherId)
    {
        ObjectiveMarker objectiveMarker = Runner.FindObject(_markerId).GetComponent<ObjectiveMarker>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);        

        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority && Runner.IsClient)
        {
            objectiveMarker.isTriggered = true;
        }
        else if (objectiveMarker != null && inputAuth != null && inputAuth.HasStateAuthority && inputAuth.HasInputAuthority && !Runner.IsClient)
        {
            objectiveMarker.isTriggered = true;
        }
    }
}
