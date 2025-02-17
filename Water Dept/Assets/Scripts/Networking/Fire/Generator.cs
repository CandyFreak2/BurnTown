using UnityEngine;
using Fusion;

public class Generator : NetworkBehaviour
{
    [SerializeField] private Objective PoweredObjective;
    [SerializeField] private Burnable genBurnable;
    [SerializeField] private float disabledPercent01; // value 0-1

    [Networked] public NetworkBool isDisabled {  get; set; }

    public override void Spawned()
    {
        PoweredObjective.AddGenerator(this);
    }

    public override void FixedUpdateNetwork()
    {
        bool lastState = isDisabled;
        isDisabled = genBurnable.GetDisabled(disabledPercent01);

        // When isDisabled switches update objective
        if (lastState != isDisabled)
        {
            PoweredObjective.CheckAllDeactivated();
        }
    }
}
