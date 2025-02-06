using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    private NetworkInputData accumulatedInput;
    private bool resetInput;

    private bool mouseButton0;

    private void Update()
    {
        mouseButton0 = mouseButton0 | Input.GetMouseButton(0);
    }

    void IBeforeUpdate.BeforeUpdate()
    {
        if (resetInput)
        {
            resetInput = false;
            accumulatedInput = default;
        }

        Keyboard keyboard = Keyboard.current;

        // Change cursor lock state if enter or esc pressed
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Get input only if cursor is locked
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        NetworkButtons buttons = default;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            Vector2 lookRotationDelta = new(-mouseDelta.y, mouseDelta.x);

            accumulatedInput.lookDelta += lookRotationDelta;
        }

        if (keyboard != null)
        {
            Vector2 moveDirection = Vector2.zero;

            if (keyboard.wKey.isPressed)
            {
                moveDirection += Vector2.up;
            }
            if (keyboard.sKey.isPressed)
            {
                moveDirection += Vector2.down;
            }
            if (keyboard.aKey.isPressed)
            {
                moveDirection += Vector2.left;
            }
            if (keyboard.dKey.isPressed)
            {
                moveDirection += Vector2.right;
            }
            accumulatedInput.direction += moveDirection;

            buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);
            buttons.Set(NetworkInputData.MOUSEBUTTON0, mouseButton0);
            mouseButton0 = false;
        }

        accumulatedInput.buttons = new NetworkButtons(accumulatedInput.buttons.Bits | buttons.Bits);
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        accumulatedInput.direction.Normalize();
        input.Set(accumulatedInput);
        resetInput = true;

        //We have to reset the look delta immediately because we don't want mouse input being reused if another tick is executed during this same frame
        accumulatedInput.lookDelta = default;
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

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
