using UnityEngine;
using UnityEngine.Events;

public class FiretruckDetection : MonoBehaviour {
    public UnityEvent OnNearbyTruck;
    public UnityEvent OnLeavingTruck;

    private bool hasPlayerNearby = false;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            OnNearbyTruck?.Invoke();
            hasPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            OnLeavingTruck?.Invoke();
            hasPlayerNearby = false;
        }
    }

    public bool HasPlayerNearby() { return hasPlayerNearby; }
}
