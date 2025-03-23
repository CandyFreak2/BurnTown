using Fusion;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EndZone : NetworkBehaviour {
    public enum EndConditions {
        RADIUS,
        FIRETRUCK,
        TIMER_END
    }

    public EndConditions triggerState = EndConditions.RADIUS; 

    [SerializeField] private float radius;
    [SerializeField] private float timeToComplete;

    public void FixedUpdate() {
        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.STARTED) {

        }
    }

    private void OnTriggerExit(Collider other) {
        
    }

    private void OnTriggerStay(Collider other) {
        
    }
}
