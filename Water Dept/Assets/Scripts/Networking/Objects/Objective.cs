using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Objective : NetworkBehaviour
{
    private List<Generator> generators;
    [SerializeField] private Animator animator;

    [Networked] private NetworkBool isComplete {  get; set; }

    private void Awake()
    {
        generators = new List<Generator>();
    }

    public override void Spawned()
    {
        isComplete = false;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        generators.Clear();
    }

    public void AddGenerator(Generator _generator)
    {
        generators.Add(_generator);
    }

    // If all generators are destroyed lower bridge
    public void CheckAllDeactivated()
    {
        bool allGeneratorsDead = true;

        foreach (Generator gen in generators)
        {
            if (!gen.isDisabled)
            {
                allGeneratorsDead = false;
            }
        }

        if (allGeneratorsDead)
        {
            RPC_DropBridge(Object.Id);
            isComplete = true;
        }
    }

    // Tells everybody to play animation
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DropBridge(NetworkId _objectiveId)
    {
        Objective objective = Runner.FindObject(_objectiveId).gameObject.GetComponent<Objective>();

        if (objective != null)
        {
            objective.animator.Play("Base Layer.BridgeAnimation", 0, 0.0f);
            objective.isComplete = true;
        }
    }
}
