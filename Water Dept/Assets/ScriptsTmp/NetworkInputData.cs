using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;

    // Movement
    public Vector3 direction;
    public NetworkButtons buttons;

    // Camera
    public float horizontalRotation;
    public float verticalRotation;
}
