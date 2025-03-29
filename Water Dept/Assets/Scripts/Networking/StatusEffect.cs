using Fusion;
using UnityEngine;

public class StatusEffect : NetworkBehaviour
{
    private PlayerMovement playerMovement;

    [Networked] private float timer { get; set; }
    private bool canMove = true;

    public float stunDuration = 3;

    public override void FixedUpdateNetwork()
    {
        if (!canMove)
        {
            timer += Time.fixedDeltaTime;
            if (timer > stunDuration)
            {
                togglePlayerMovement(true);
                canMove = true;
                timer = 0;
            }
        }
        Debug.Log(canMove);
    }

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
        canMove = !canMove;
        RPC_SetInputState(isStunned);
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetInputState(bool enable, RpcInfo info = default)
    {
        playerMovement = Runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
        playerMovement.enabled = !enable;
    }
}