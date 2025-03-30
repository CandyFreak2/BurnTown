using Fusion;
using UnityEngine;

public enum EndConditions {
    RADIUS,
    FIRETRUCK
}

public class EndZone : NetworkBehaviour {

    public EndConditions state = EndConditions.RADIUS;

    [Header("Radius Conditions")]
    [Networked] private TickTimer Timer { get; set; }
    [Tooltip("TTC in Seconds"), Networked, SerializeField] private float timeToComplete { get; set; }
    private int playersInRadius = 0;

    [Header("Firetruck Conditions")]
    [Tooltip("Only need if firetruck option is selected"), SerializeField] private CarData firetruck;
    
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
                        GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.END);
                        EventManager.EndZoneTrigger(state);
                    }
                }

                if (state == EndConditions.FIRETRUCK) {
                    if (firetruck == null) {
                        Debug.LogWarning(gameObject.name + "needs Firetruck's Car Data");
                    } else if (firetruck != null) {
                        if (firetruck.GetNumPassengers() == ServerInfo.MaxPlayers) {
                            GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.END);
                            EventManager.EndZoneTrigger(state);
                        }
                    } 
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius++;
            EventManager.EndZoneEnter(state);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius--;
            EventManager.EndZoneExit(state);
        }
    }
}