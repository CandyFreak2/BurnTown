using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class StartZone : NetworkBehaviour {
    private NetworkBool isSpawned { get; set; }
    [SerializeField] private List<GameObject> players = new();

    public override void Spawned() {
        isSpawned = true;
    }

    public void FixedUpdate() {
        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED) {
            if (players.Count == ServerInfo.MaxPlayers && IsAllTrue(players)) {
                GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.STARTED);
                EventManager.StartZoneTrigger();
            }
        }
    }

    public bool IsAllTrue(List<GameObject> collection) {
        for (int i = 0; i < collection.Count; i++)
            if (!collection[i].GetComponentInParent<PlayerMovement>().IsReady) {
                return false;
            }
        return true;
    }

    private void OnTriggerExit(Collider other) {
        if (isSpawned && GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED) {
            if (other.CompareTag("Player") && !players.Contains(other.gameObject)) {
                players.Add(other.gameObject);
                other.GetComponentInParent<PlayerMovement>().IsReady = true;
                EventManager.StartZoneExit();
            }
        }
    }
}
