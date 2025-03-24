using Fusion;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EndZone : NetworkBehaviour {
    public enum EndConditions {
        RADIUS,
        FIRETRUCK,
        TIMER_END
    }

    public EndConditions state = EndConditions.RADIUS;

    [Networked] private TickTimer Timer { get; set; }
    [Networked, SerializeField] private float timeToComplete { get; set; }
    private int playersInRadius = 0;
    private bool canStart = false;

    public override void Spawned() {
        canStart = true;
    }

    public void FixedUpdate() {
        if (canStart) {
            if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.STARTED) {
                if (state == EndConditions.RADIUS && playersInRadius == ServerInfo.MaxPlayers) {
                    if (!Timer.IsRunning) {
                        Timer = TickTimer.CreateFromSeconds(Runner, timeToComplete);
                    } else if (Timer.ExpiredOrNotRunning(Runner)) {
                        Timer = TickTimer.None;
                        Debug.Log("Timer Ended");
                        GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.END);
                        
                        // UnityEvent that gets called onTrigger when for enter, exit and trigger
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius++;
            Debug.Log(playersInRadius);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius--;
        }
    }


    private void OnTriggerStay(Collider other) {
        //if (other.CompareTag("Player") && state == EndConditions.RADIUS) {
        //    playersInRadius++;
        //    Debug.Log(playersInRadius);
        //}

    }
}