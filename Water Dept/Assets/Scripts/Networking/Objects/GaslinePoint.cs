using UnityEngine;

public class GaslinePoint : MonoBehaviour {
    public bool isOnFire = false;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Flame")) {
            isOnFire = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Flame")) {
            isOnFire = false;
        }
    }

    
    public bool GetIsOnFire() => isOnFire;
}
