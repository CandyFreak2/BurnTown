using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;

public class InteractableSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef hosePrefab;

    [SerializeField] private List<Vector3> hosePos;
    [SerializeField] private int numHoses;

    private readonly HashSet<NetworkObject> _spawnedObjects = new HashSet<NetworkObject>();

    public override void Spawned()
    {
        InitializeScene();
    }

    private void InitializeScene()
    {
        //INitialize scene
        for (int i = 0; i < numHoses; i++)
        {
            SpawnHose(hosePos[i]);
        }
    }

    public void SpawnHose(Vector3 _spawnPos)
    {
        if (Runner.IsClient) return; // Clients can't spawn.

        var obj = Runner.Spawn(hosePrefab, _spawnPos);
        _spawnedObjects.Add(obj);
    }
}
