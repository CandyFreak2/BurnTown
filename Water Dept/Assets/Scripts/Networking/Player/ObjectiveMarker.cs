using Fusion;
using UnityEngine;

public class ObjectiveMarker : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue = 6; // Im sry Jerry :(
    [Networked] public bool isTriggered { get; set; }

    
    public override void Spawned()
    {
        isTriggered = false;
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkId otherId = other.transform.parent.GetComponent<NetworkObject>().Id;

            RPC_TriggerMarker(Object.Id, otherId);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_TriggerMarker(NetworkId _markerId, NetworkId _otherId)
    {
        ObjectiveMarker objectiveMarker = Runner.FindObject(_markerId).GetComponent<ObjectiveMarker>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);

        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority)
        {
            objectiveMarker.isTriggered = true;
        }
    }
}
