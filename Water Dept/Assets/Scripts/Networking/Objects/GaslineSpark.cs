using Fusion;
using UnityEngine;

public class GaslineSpark : NetworkBehaviour {
    [Networked] public bool isOnGaslinePoint { get; set; }

    // Network
    private bool isSpawned = false;

    public override void Spawned() {
        isSpawned = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("GaslinePoint") && isSpawned) {
            isOnGaslinePoint = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("GaslinePoint") && isSpawned) {
            isOnGaslinePoint = false;
        }
    }

    
    public bool GetIsOnGaslinePoint() => isOnGaslinePoint;
}
