using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class StartZone : NetworkBehaviour {
    private NetworkBool isSpawned { get; set; }
    [SerializeField] private List<GameObject> players = new();

    [Header("Timer")]
    [Networked] public TickTimer Timer { get; set; }
    [Tooltip("Duration (Seconds)"), SerializeField] private float duration;
    [SerializeField] private int playersInRadius = 0;

    public override void Spawned() {
        isSpawned = true;
    }

    public void FixedUpdate() {
        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED) {
            if (players.Count >= ServerInfo.MaxPlayers && IsAllTrue(players)) {
                GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.STARTED);
                EventManager.StartZoneTrigger();
            }
        }

        if (isSpawned) {
            if (playersInRadius >= ServerInfo.MaxPlayers && !Timer.IsRunning) {
                Timer = TickTimer.CreateFromSeconds(Runner, duration);
                EventManager.StartZoneTimerStart();
            }

            if (playersInRadius < ServerInfo.MaxPlayers && Timer.IsRunning) {
                Timer = TickTimer.None;
                EventManager.StartZoneTimerReset();
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

    private void OnTriggerEnter(Collider other) {
        if (isSpawned) {
            if (other.CompareTag("Player")) {
                playersInRadius++;
                if (!players.Contains(other.gameObject) && GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED) {
                    players.Add(other.gameObject);
                    other.GetComponentInParent<PlayerMovement>().IsReady = true;
                    EventManager.StartZoneExit();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (isSpawned && other.CompareTag("Player")) {
            playersInRadius--;
        }
    }

    public float GetTimeLeft => Timer.RemainingTime(Runner) ?? 0;
}
