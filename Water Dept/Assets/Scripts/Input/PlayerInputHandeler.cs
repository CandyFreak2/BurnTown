using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandeler : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    InputSystem_Actions inputSystem;

    Vector2 moveDirection;
    Vector2 lookDelta;

    public PlayerInputAction accumulatedInput = new PlayerInputAction();
    private bool resetInput = false;
    public Action<PlayerInputAction> onInputSignal;

    private void Awake()
    {
        inputSystem = new InputSystem_Actions();

        //get input and leave it hanging
        inputSystem.Player.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        inputSystem.Player.Look.performed += ctx => lookDelta = ctx.ReadValue<Vector2>();

        //clear input
        inputSystem.Player.Move.canceled += ctx => moveDirection = ctx.ReadValue<Vector2>();
        //inputSystem.Player.Jump.started += spaghetti => accumulatedInput.onInteractStarted?.Invoke();
    }

    private void OnEnable() //restarts controls when needed
    {
        inputSystem.Enable();
    }

    private void OnDisable() //prevents controls running in editor and making junk
    {
        inputSystem.Disable();

        moveDirection = Vector2.zero;
        lookDelta = Vector2.zero;
    }

    private void Start()
    {
        onInputSignal?.Invoke(new PlayerInputAction(accumulatedInput));
    }

    private void Update()
    {
        ////load inputs into the package
        //accumulatedInput.MoveDirection = moveDirection;
        
        //if(lookDelta.magnitude > 0)
        //    accumulatedInput.LookDelta = lookDelta;

        ////send out a copy of this frame's input to all listeners?
        //onInputSignal?.Invoke(new PlayerInputAction(accumulatedInput));
    }

    // Networking functions
    void IBeforeUpdate.BeforeUpdate()
    {
        if (resetInput)
        {
            resetInput = false;
            accumulatedInput = default;
        }

        // Add buttons
        NetworkButtons buttons = default;
        Keyboard keyboard = Keyboard.current;
        buttons.Set(InputButton.Interact, keyboard.eKey.isPressed);

        Mouse mouse = Mouse.current;
        buttons.Set(InputButton.Shoot, mouse.leftButton.isPressed);

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

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Send package to server
        accumulatedInput.MoveDirection.Normalize();
        input.Set(accumulatedInput);
        resetInput = true;

        //We have to reset the look delta immediately because we don't want mouse input being reused if another tick is executed during this same frame
        accumulatedInput.LookDelta = default;
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
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

    async void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
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
