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
        isTriggered = false;
    }

    public override void Spawned()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject otherNetworkObj = other.transform.parent.GetComponent<NetworkObject>();
            PlayerMovement playerMovement = other.transform.parent.GetComponent<PlayerMovement>();
            PlayerRef playerRef = playerMovement.playerRef;
            
            RPC_TriggerMarker(playerRef, Object.Id, otherNetworkObj.Id);
        }
    }

    //[Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    [Rpc(RpcSources.All, RpcTargets.All)] // Run by player with input auth
    public void RPC_TriggerMarker([RpcTarget] PlayerRef _player, NetworkId _markerId, NetworkId _otherId)
    {
        ObjectiveMarker objectiveMarker = Runner.FindObject(_markerId).GetComponent<ObjectiveMarker>();
        NetworkObject inputAuth = Runner.FindObject(_otherId);
        Debug.Log("In Rpc tho " + _player);

        if (objectiveMarker != null && inputAuth != null && inputAuth.HasInputAuthority)
        {
            Debug.Log("In if tho");
            objectiveMarker.isTriggered = true;
        }
    }
}
