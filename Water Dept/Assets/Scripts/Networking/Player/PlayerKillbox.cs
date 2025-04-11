using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerKillbox : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue;

    private void Start()
    {
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2); //Log base 2 to find player layer because LayerMask is a bitmask
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject otherNetworkObj = other.transform.parent.GetComponent<NetworkObject>();

            RPC_RespawnPlayer(otherNetworkObj.Id);
        }
    }

    //[Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] // Run by player with input auth
    public void RPC_RespawnPlayer(NetworkId _otherId)
    {
        NetworkObject playerNetworkObj = Runner.FindObject(_otherId);
        SimpleKCC playerKCC = playerNetworkObj.GetComponent<SimpleKCC>();

        if (playerKCC != null)
        {
            playerKCC.SetPosition(ObjectiveIndicatorList.Singleton.GetLastObjectivePos() + Vector3.up);
        }
    }
}
