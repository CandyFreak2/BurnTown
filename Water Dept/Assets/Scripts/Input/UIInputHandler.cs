using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInputHandler : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks {
    private InputSystem_Actions.UIActions UIInputSystem;
    private bool isPanelOn = false;

    public static Action<bool> onActionPause;
    public static Action<InputButton> onActionInteract;

    private void Awake() {
        UIInputSystem = new InputSystem_Actions().UI;
        UIInputSystem.Pause.started += ctx => OnPause(isPanelOn);
        UIInputSystem.Interact.started += ctx => OnInteract(InputButton.Interact);
        UIInputSystem.Drop.started += ctx => OnInteract(InputButton.Drop);
        UIInputSystem.Attack.started += ctx => OnInteract(InputButton.ShootDown);
        UIInputSystem.Brake.started += ctx => OnInteract(InputButton.BrakeDown);
        UIInputSystem.Interact.canceled += ctx => OnInteract(InputButton.Interact);
        UIInputSystem.Drop.canceled += ctx => OnInteract(InputButton.Drop);
        UIInputSystem.Attack.canceled += ctx => OnInteract(InputButton.ShootUp);
        UIInputSystem.Brake.canceled += ctx => OnInteract(InputButton.BrakeUp);
    }

    private void OnPause(bool input) {
        onActionPause?.Invoke(input);
        isPanelOn = !isPanelOn;
    }

    private void OnInteract(InputButton input)
    {
        onActionInteract?.Invoke(input);
    }

    private void OnEnable() {
        UIInputSystem.Enable();
    }

    private void OnDisable() {
        UIInputSystem.Disable();
    }

    #region unnecessary

    public void BeforeUpdate() {
        throw new System.NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner) {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner) {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {
        throw new NotImplementedException();
    }
    #endregion
}
