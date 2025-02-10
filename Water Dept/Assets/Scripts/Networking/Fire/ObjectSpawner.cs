using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef treePrefab;
    [SerializeField] private NetworkPrefabRef buildingPrefab1;
    [SerializeField] private NetworkPrefabRef buildingPrefab2;

    [SerializeField] private int numTrees;
    [SerializeField] private int numSmallBuildings;
    [SerializeField] private int numBigBuildings;
    [SerializeField] private int spawnRadius;

    private readonly HashSet<NetworkObject> _spawnedObjects = new HashSet<NetworkObject>();

    public override void Spawned()
    {
        //InitializeScene();
    }

    private void InitializeScene()
    {
        //INitialize scene
        for (int i = 0; i < numTrees; i++)
        {
            SpawnTree();
        }

        for (int i = 0; i < numSmallBuildings; i++)
        {
            SpawnSmallBuilding();
        }

        for (int i = 0; i < numSmallBuildings; i++)
        {
            SpawnBigBuilding();
        }
    }

    public void SpawnTree()
    {
        if (Runner.IsClient) return; // Clients can't spawn.

        var obj = Runner.Spawn(treePrefab, transform.position + (new Vector3(Random.insideUnitCircle.x, 0.0f, Random.insideUnitCircle.y) * spawnRadius));
        _spawnedObjects.Add(obj);
    }

    public void SpawnSmallBuilding()
    {
        if (Runner.IsClient) return; // Clients can't spawn.

        var obj = Runner.Spawn(buildingPrefab1, transform.position + (new Vector3(Random.insideUnitCircle.x, 0.0f, Random.insideUnitCircle.y) * spawnRadius));
        _spawnedObjects.Add(obj);
    }

    public void SpawnBigBuilding()
    {
        if (Runner.IsClient) return; // Clients can't spawn.

        var obj = Runner.Spawn(buildingPrefab2, transform.position + (new Vector3(Random.insideUnitCircle.x, 0.0f, Random.insideUnitCircle.y) * spawnRadius));
        _spawnedObjects.Add(obj);
    }

    public void DespawnAll()
    {
        if (Runner.IsClient) return; // Clients can't despawn.

        if (_spawnedObjects == null) return;

        foreach (var networkObject in _spawnedObjects)
        {
            Runner.Despawn(networkObject);
        }

        _spawnedObjects.Clear();
    }
}
