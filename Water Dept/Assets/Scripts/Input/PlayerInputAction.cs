using Fusion;
using System;
using UnityEngine;

public enum InputButton
{
    Interact, 
    Shoot,
    Drop,
    Brake,
    Throw
}

public struct PlayerInputAction : INetworkInput
{
    public Vector2 MoveDirection;
    public Vector2 LookDelta;

    public NetworkButtons Buttons;

    public PlayerInputAction(PlayerInputAction action)
    {
        MoveDirection = action.MoveDirection;
        LookDelta = action.LookDelta;
        Buttons = action.Buttons;
    }
}
