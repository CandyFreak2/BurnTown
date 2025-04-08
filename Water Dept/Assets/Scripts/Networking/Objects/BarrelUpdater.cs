using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class BarrelUpdater : NetworkBehaviour
{
    [SerializeField] private Burnable burnable;
    private Rigidbody rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && burnable.nIsBurning)
        {
            rb.WakeUp();
        }
    }
}
