using Fusion;
using UnityEngine;

public class StatusEffect : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    
    private void OnEnable()
    {
        EventManager.onPlayerStunned += togglePlayerMovement;
    }

    private void OnDisable()
    {
        EventManager.onPlayerStunned -= togglePlayerMovement;
    }

    public void togglePlayerMovement(bool isStunned)
    {
        isStunned = !isStunned;
        RPC_SetInputState(isStunned);
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetInputState(bool enable, RpcInfo info = default)
    {
        playerMovement = Runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
        playerMovement.enabled = !enable;
    }
}
