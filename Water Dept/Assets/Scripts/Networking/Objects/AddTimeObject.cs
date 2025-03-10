using Fusion;
using UnityEngine;

public class AddTimeObject : NetworkBehaviour {
    [SerializeField, Tooltip("Duration (Seconds)")] private float increment;

    private void OnDisable() => EventManager.TimerAdd(increment);
}
