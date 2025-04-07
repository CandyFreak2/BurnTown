using Fusion;
using System;
using UnityEngine;

public enum InputButton
{
    Interact, 
    Shoot,
    Drop,
    Brake,
    Throw,
    ShootUp,
    ShootDown,
    BrakeDown,
    BrakeUp// Should have done this to begin with but just putting in now to get ui animations networked:( - jake
}

public struct PlayerInputAction : INetworkInput
{
    public Vector2 MoveDirection;
    public Vector2 LookDelta;
    public Vector2 CameraDirection;

    public NetworkButtons Buttons;

    public PlayerInputAction(PlayerInputAction action)
    {
        MoveDirection = action.MoveDirection;
        LookDelta = action.LookDelta;
        CameraDirection = action.CameraDirection;
        Buttons = action.Buttons;
    }
}
