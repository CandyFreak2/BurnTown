using Fusion;
using UnityEngine;

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
