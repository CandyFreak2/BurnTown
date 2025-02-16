using UnityEngine;
using UnityEngine.Events;

public class Firetruck : MonoBehaviour {
    public UnityEvent IsCloseToTruck;
    public UnityEvent OnLeavingTruck;

    private bool isPlayerNearby;
    private FiretruckDetection nearbyRadiusDetection;

    private void Awake() {
        nearbyRadiusDetection = GetComponentInChildren<FiretruckDetection>();
    }

    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            IsCloseToTruck?.Invoke();
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            OnLeavingTruck?.Invoke();
            isPlayerNearby = false;
        }
    }

    public bool IsPlayerNearby() { return isPlayerNearby; }
    public bool IsPlayerInLargerRadius() { return nearbyRadiusDetection.HasPlayerNearby(); }
}
