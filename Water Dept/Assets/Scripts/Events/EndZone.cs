using Fusion;
using UnityEngine;
using UnityEngine.Events;
using static GameStateHandler;

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

    public UnityEvent TriggerEndZone;

    public override void Spawned() {
        canStart = true;
    }

    public void FixedUpdate() {
        if (canStart) {
            if (Instance.GetGameState == GameState.STARTED) {
                if (state == EndConditions.RADIUS && playersInRadius == ServerInfo.MaxPlayers) {
                    if (!Timer.IsRunning) {
                        Timer = TickTimer.CreateFromSeconds(Runner, timeToComplete);
                    } else if (Timer.ExpiredOrNotRunning(Runner)) {
                        Timer = TickTimer.None;
                        RPC_TriggerEndSignal();
                    }
                }

                /*if (state == EndConditions.FIRETRUCK) {
                    if (firetruck == null) {
                        Debug.LogWarning(gameObject.name + "needs Firetruck's Car Data");
                    } else {
                        if (firetruck.GetNumPassengers() == ServerInfo.MaxPlayers) {
                            Instance.SetGameState(GameState.END);
                            EventManager.EndZoneTrigger(state);
                        }
                    } 
                }*/
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius++;
            EventManager.EndZoneEnter(state);
        }

        //this is more like what I needed -James
        if (state == EndConditions.FIRETRUCK && other.CompareTag("FireTruck"))
        {
            RPC_TriggerEndSignal();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (state == EndConditions.RADIUS && other.CompareTag("Player")) {
            playersInRadius--;
            EventManager.EndZoneExit(state);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_TriggerEndSignal() => RPC_TriggerEndRelay();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_TriggerEndRelay() => TriggerEnd();

    void TriggerEnd()
    {
        TriggerEndZone?.Invoke();
        Instance.SetGameState(GameState.END);
        EventManager.EndZoneTrigger(state);
    }
}