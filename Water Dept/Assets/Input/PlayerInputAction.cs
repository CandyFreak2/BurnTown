using Fusion;
using UnityEngine;

public struct PlayerInputAction : INetworkInput
{
    public Vector2 MoveDirection;
    public Vector2 LookDelta;

    //public PlayerInputAction()
    //{
    //    MoveDirection = Vector2.zero;
    //    LookDelta = Vector2.zero;
    //}

    public PlayerInputAction(PlayerInputAction action)
    {
        MoveDirection = action.MoveDirection;
        LookDelta = action.LookDelta;
    }
}
