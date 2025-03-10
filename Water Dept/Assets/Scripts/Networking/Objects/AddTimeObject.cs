using Fusion;
using UnityEngine;

public class AddTimeObject : NetworkBehaviour {
    [SerializeField] private float increment;

    private void OnDisable() => EventManager.TimerAdd(increment);
}
