using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.SceneManagement;

public enum ConnectionStatus {
    DISCONNECTED,
    CONNECTING,
    FAILED,
    CONNECTED
}

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks {
    // Rework to contain own class
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private Firefighter fireFighters;

    public static ConnectionStatus status = ConnectionStatus.DISCONNECTED;

    private GameMode gameMode;
    private NetworkRunner _runner;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private bool mouseButton0;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if(runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            runner.SetPlayerObject(player, networkPlayerObject);
        }
        /*if (runner.IsServer) {
            if (gameMode == GameMode.Host) {
                runner.Spawn(managerPrefab, Vector3.zero, Quaternion.identity);
            }
            var fireFighter = runner.Spawn(fireFighters, Vector3.zero, Quaternion.identity, player);
            fireFighter.GameState = Firefighter.PlayerState.LOBBY;
        }*/
        SetConnectionStatus(ConnectionStatus.CONNECTED);
        UIManager.Instance.SetUI();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }

        Firefighter.RemovePlayer(runner, player);
        SetConnectionStatus(status);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Debug.Log($"OnShutdown {shutdownReason}");
        SetConnectionStatus(ConnectionStatus.DISCONNECTED);

        (string status, string message) = OutputShutdownReason(shutdownReason);
        //Firefighter.players.Clear();
        _spawnedCharacters.Clear();

        if (_runner) {
            Destroy(_runner.gameObject);
        }

        _runner = null;
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    async void StartGame(GameMode mode) {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs() {
            GameMode = mode,
            SessionName = "TestRoom-3",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void Update() {
        mouseButton0 = mouseButton0 | Input.GetMouseButton(0);
    }

    private void SetConnectionStatus(ConnectionStatus nStatus) {
        Debug.Log($"Setting connection status to {nStatus}");

        status = nStatus;

        if (!UnityEngine.Application.isPlaying) return;

        if (nStatus == ConnectionStatus.DISCONNECTED || nStatus == ConnectionStatus.FAILED) {
            SceneManager.LoadScene(0);
        }
    }

    private static (string, string) OutputShutdownReason(ShutdownReason reason) {
        switch (reason) {
            case ShutdownReason.Ok:
                return (null, null);
            case ShutdownReason.Error:
                return ("Error", "Shutdown was caused by some internal error");
            case ShutdownReason.IncompatibleConfiguration:
                return ("Incompatible Config", "Mismatching type between client Server Mode and Shared Mode");
            case ShutdownReason.ServerInRoom:
                return ("Room name in use", "There's a room with that name! Please try a different name or wait a while.");
            case ShutdownReason.DisconnectedByPluginLogic:
                return ("Disconnected By Plugin Logic", "You were kicked, the room may have been closed");
            case ShutdownReason.GameClosed:
                return ("Game Closed", "The session cannot be joined, the game is closed");
            case ShutdownReason.GameNotFound:
                return ("Game Not Found", "This room does not exist");
            case ShutdownReason.MaxCcuReached:
                return ("Max Players", "The Max CCU has been reached, please try again later");
            case ShutdownReason.InvalidRegion:
                return ("Invalid Region", "The currently selected region is invalid");
            case ShutdownReason.GameIdAlreadyExists:
                return ("ID already exists", "A room with this name has already been created");
            case ShutdownReason.GameIsFull:
                return ("Game is full", "This lobby is full!");
            case ShutdownReason.InvalidAuthentication:
                return ("Invalid Authentication", "The Authentication values are invalid");
            case ShutdownReason.CustomAuthenticationFailed:
                return ("Authentication Failed", "Custom authentication has failed");
            case ShutdownReason.AuthenticationTicketExpired:
                return ("Authentication Expired", "The authentication ticket has expired");
            case ShutdownReason.PhotonCloudTimeout:
                return ("Cloud Timeout", "Connection with the Photon Cloud has timed out");
            default:
                Debug.LogWarning($"Unknown ShutdownReason {reason}");
                return ("Unknown Shutdown Reason", $"{(int)reason}");
        }
    }

    private static (string, string) ConnectFailedReason(NetConnectFailedReason reason) {
        switch (reason) {
            case NetConnectFailedReason.Timeout:
                return ("Timed Out", "");
            case NetConnectFailedReason.ServerRefused:
                return ("Connection Refused", "The lobby may be currently in-game");
            case NetConnectFailedReason.ServerFull:
                return ("Server Full", "");
            default:
                Debug.LogWarning($"Unknown NetConnectFailedReason {reason}");
                return ("Unknown Connection Failure", $"{(int)reason}");
        }
    }

    public Dictionary<PlayerRef, NetworkObject> GetSpawnedPlayers() => _spawnedCharacters;
}
