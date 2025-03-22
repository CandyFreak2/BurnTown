using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;

public interface IBasicSpawner {
    Dictionary<PlayerRef, NetworkObject> GetSpawnedPlayers();
    void JoinOrCreateLobby();
    void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data);
    void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken);
    void OnInput(NetworkRunner runner, NetworkInput input);
    void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input);
    void OnPlayerJoined(NetworkRunner runner, PlayerRef player);
    void OnPlayerLeft(NetworkRunner runner, PlayerRef player);
    void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress);
    void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data);
    void OnSceneLoadDone(NetworkRunner runner);
    void OnSceneLoadStart(NetworkRunner runner);
    void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList);
    void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason);
    void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message);
}