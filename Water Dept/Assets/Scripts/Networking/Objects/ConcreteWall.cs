using Fusion;

public class ConcreteWall : NetworkBehaviour {
    private bool isSpawned;

    public override void Spawned() {
        isSpawned = true;
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_Explode() {
        if (Object.HasStateAuthority && isSpawned) {
            Runner.Despawn(Object);
        }
    }
}
