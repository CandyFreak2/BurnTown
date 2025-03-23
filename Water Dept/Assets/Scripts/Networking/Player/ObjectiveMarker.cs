using Fusion;
using UnityEngine;

public class ObjectiveMarker : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue;
    [Networked] public bool isTriggered { get; set; }

    private void Start()
    {
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2);
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
            PlayerRef playerRef = other.transform.parent.GetComponent<PlayerMovement>().playerRef;

            RPC_TriggerMarker(playerRef, Object.Id, otherNetworkObj.Id);
        }
    }

    //[Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    //[Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_TriggerMarker([RpcTarget] PlayerRef _player, NetworkId _markerId, NetworkId _otherId)
    {
        ObjectiveMarker objectiveMarker = Runner.FindObject(_markerId).GetComponent<ObjectiveMarker>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);

        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority)
        {
            objectiveMarker.isTriggered = true;
        }
    }
}
