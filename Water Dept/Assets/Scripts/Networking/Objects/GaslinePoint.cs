using Fusion;
using UnityEngine;

public class GaslinePoint : NetworkBehaviour {
    [Networked] public bool isOnFire { get; set; }

    // Network
    private bool isSpawned = false;

    public override void Spawned() {
        isSpawned = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Flame") && isSpawned) {
            isOnFire = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Flame") && isSpawned) {
            isOnFire = false;
        }
    }

    
    public bool GetIsOnFire() => isOnFire;
}
