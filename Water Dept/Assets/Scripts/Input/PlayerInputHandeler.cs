using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandeler : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks {
    private enum InputProfiles {
        PLAYER,
        UI
    };

    InputSystem_Actions playerInputSystem;
    InputSystem_Actions UIInputSystem;

    Vector2 moveDirection;
    Vector2 lookDelta;
    float interactPress;
    float shootPress;


    public PlayerInputAction accumulatedInput = new PlayerInputAction();
    private bool resetInput = false;
    public Action<PlayerInputAction> onInputSignal;

    private void Awake() {
        playerInputSystem = new InputSystem_Actions();

        //get input and leave it hanging
        playerInputSystem.Player.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        playerInputSystem.Player.Look.performed += ctx => lookDelta = ctx.ReadValue<Vector2>();

        //interact
        playerInputSystem.Player.Interact.started += ctx => interactPress = ctx.ReadValue<float>();

        //shoot
        playerInputSystem.Player.Attack.performed += ctx => shootPress = ctx.ReadValue<float>();

        //clear input
        playerInputSystem.Player.Move.canceled += ctx => moveDirection = ctx.ReadValue<Vector2>();
        //inputSystem.Player.Jump.started += spaghetti => accumulatedInput.onInteractStarted?.Invoke();

        //inputSystem.UI.Pause.started += ctx => ;
    }

    private void Navigate_performed(InputAction.CallbackContext obj) {
        throw new NotImplementedException();
    }

    private void OnEnable() //restarts controls when needed
    {
        playerInputSystem.Enable();
        UIInputSystem.Enable();
    }

    private void OnDisable() //prevents controls running in editor and making junk
    {
        playerInputSystem.Disable();
        UIInputSystem.Disable();
    }

    private void Start() {
        onInputSignal?.Invoke(new PlayerInputAction(accumulatedInput));
    }

    // Networking functions
    void IBeforeUpdate.BeforeUpdate() {
        if (resetInput) {
            resetInput = false;
            accumulatedInput = default;
        }

        // Add buttons
        NetworkButtons buttons = default;

        bool interactDown = (playerInputSystem.Player.Interact.ReadValue<float>() > 0.0f);

        buttons.Set(InputButton.Interact, interactDown);

        // shootpress returns 0, attack.readvalue returns 1
        bool shootDown = (playerInputSystem.Player.Attack.ReadValue<float>() > 0.0f);

        buttons.Set(InputButton.Shoot, shootDown);

        // Change cursor lock state if enter or esc pressed
        //if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
        //{
        //    if (Cursor.lockState == CursorLockMode.Locked)
        //    {
        //        Cursor.lockState = CursorLockMode.None;
        //        Cursor.visible = true;
        //    }
        //    else
        //    {
        //        Cursor.lockState = CursorLockMode.Locked;
        //        Cursor.visible = false;
        //    }
        //}

        //load inputs into the package
        accumulatedInput.MoveDirection += moveDirection;

        if (lookDelta.magnitude > 0)
            accumulatedInput.LookDelta += lookDelta;

        accumulatedInput.Buttons = new NetworkButtons(accumulatedInput.Buttons.Bits | buttons.Bits);
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) {
        // Send package to server
        accumulatedInput.MoveDirection.Normalize();
        input.Set(accumulatedInput);
        resetInput = true;

        //We have to reset the look delta immediately because we don't want mouse input being reused if another tick is executed during this same frame
        accumulatedInput.LookDelta = default;
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        //if (player == runner.LocalPlayer)
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    async void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
        //{
        //    await FindFirstObjectByType<MenuConnectionBehaviour>(FindObjectsInactive.Include).DisconnectAsync(ConnectFailReason.Disconnect);
        //    FindFirstObjectByType<FusionMenuUIGameplay>(FindObjectsInactive.Include).Controller.Show<FusionMenuUIMain>();
        //}
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
