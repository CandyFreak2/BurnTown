using System;
using UnityEngine;

public class TutorialEvents : MonoBehaviour {
    public static TutorialEvents instance;

    private void Awake() {
        instance = this;
    }

    public event Action onMovementTrigger;

    public void OnMovementTrigger() {
        onMovementTrigger?.Invoke();
    }
}
