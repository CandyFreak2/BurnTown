using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Fusion.Behaviour, INetworkObjectProvider
{
    // If true, the provider will delay getting the prefab instance if the SceneManager is busy
    [InlineHelp] public bool shouldDelay = true;

    private Dictionary<NetworkPrefabId, Queue<NetworkObject>> free = new();

    // Defines how many objects can be kept on the pools, <=0 will pool all released objects
    private int maxPoolCount = 0;

    protected NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab, NetworkPrefabId contextID) {
        var result = default(NetworkObject);

        if (free.TryGetValue(contextID, out var networkObjects)) {
            if (networkObjects.Count > 0) {
                result = networkObjects.Dequeue();
                result.gameObject.SetActive(true);
                return result;
            }
        } else {
            free.Add(contextID, new Queue<NetworkObject>());
        }

        result = Instantiate(prefab);

        return result;
    }

    protected void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId contextId, NetworkObject instance) {
        if (!free.TryGetValue(contextId, out var networkObjects)) {
            // No free queue for the prefab. IS DESTROYED!!!
            Destroy(instance.gameObject);
            return;
        } else if (maxPoolCount > 0 && networkObjects.Count >= maxPoolCount) {
            // Max amount of objects, destroyed
            Destroy(instance.gameObject);
            return;
        }

        networkObjects.Enqueue(instance);
        instance.gameObject.SetActive(false);
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context) {
        var instance = context.Object;

        // Only pool prefabs
        if (!context.IsBeingDestroyed) {
            if (context.TypeId.IsPrefab) {
                DestroyPrefabInstance(runner, context.TypeId.AsPrefabId, instance);
            } else {
                 Destroy(instance.gameObject);
            }

            if (context.TypeId.IsPrefab) {
                runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
            }
        }
    }

    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance) {
        instance = null;

        if (shouldDelay && runner.SceneManager.IsBusy) {
            return NetworkObjectAcquireResult.Retry;
        }

        NetworkObject prefab;
        try {
            prefab = runner.Prefabs.Load(context.PrefabId, isSynchronous: context.IsSynchronous);
        } catch (Exception e) {
            Log.Error($"Failed to load prefab: {e}");
            return NetworkObjectAcquireResult.Failed;
        }

        if (!prefab) {
            return NetworkObjectAcquireResult.Retry;
        }

        instance = InstantiatePrefab(runner, prefab, context.PrefabId);
        Assert.Check(instance);

        if (context.DontDestroyOnLoad) {
            runner.MakeDontDestroyOnLoad(instance.gameObject);
        } else {
            runner.MoveToRunnerScene(instance.gameObject);
        }

        runner.Prefabs.AddInstance(context.PrefabId);
        return NetworkObjectAcquireResult.Success;
    }

    public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid) {
        throw new System.NotImplementedException();
    }

    public void SetMaxPoolCount(int count) { maxPoolCount = count; }
}
