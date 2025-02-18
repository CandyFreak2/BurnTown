using UnityEngine;

public class GaslineSpark : MonoBehaviour {
    private bool isOnGaslinePoint = false;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("GaslinePoint")) {
            isOnGaslinePoint = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("GaslinePoint")) {
            isOnGaslinePoint = false;
        }
    }

    public bool GetIsOnGaslinePoint() => isOnGaslinePoint;
}
