using Fusion;
using UnityEngine;

public enum InputButtonTmp
{
    Jump,
}

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;

    // Movement
    public Vector2 direction;
    public NetworkButtons buttons;

    // Camera
    public Vector2 lookDelta;
}
