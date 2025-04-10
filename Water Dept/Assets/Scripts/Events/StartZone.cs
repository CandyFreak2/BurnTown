using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class StartZone : NetworkBehaviour {
    private NetworkBool isSpawned { get; set; }
    [SerializeField] private List<GameObject> players = new();

    [Header("Timer")]
    [Networked] public TickTimer Timer { get; set; }
    [Tooltip("Duration (Seconds)"), SerializeField] private float duration;
    [Networked] private int playersInRadius { get; set; }

    public override void Spawned() {
        isSpawned = true;
    }

    public void FixedUpdate() {
        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED) {
            if (players.Count >= ServerInfo.MaxPlayers && IsAllTrue(players)) {
                GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.STARTED);
                EventManager.StartZoneTrigger(); //This just aint working right -James
            }
        }

        //currently not working 
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
        if (isSpawned && other.CompareTag("Player")){
            RPC_PlayerEnterStart(Object.Id, other.transform.parent.GetComponent<PlayerMovement>().Object.Id);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (isSpawned && other.CompareTag("Player")) {
            RPC_PlayerExitStart(Object.Id);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayerEnterStart(NetworkId _startZoneId, NetworkId _otherId)
    {
        StartZone startZone = Runner.FindObject(_startZoneId).GetComponent<StartZone>();
        PlayerMovement playerMovement = Runner.FindObject(_otherId).GetComponent<PlayerMovement>();

        if (startZone != null && playerMovement != null)
        {
            if (HasStateAuthority)
            {
                startZone.playersInRadius++;
            }
            if (!startZone.players.Contains(playerMovement.gameObject) && GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.NOT_STARTED)
            {
                startZone.players.Add(playerMovement.gameObject);
                playerMovement.IsReady = true;
                EventManager.StartZoneEnter();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayerExitStart(NetworkId _startZoneId)
    {
        StartZone startZone = Runner.FindObject(_startZoneId).GetComponent<StartZone>();

        if (startZone != null && HasStateAuthority) 
        {
            startZone.playersInRadius--;
        }
    }

    public float GetTimeLeft => Timer.RemainingTime(Runner) ?? duration;
    public int GetCurrentPlayers => playersInRadius; //does not sync for clients -James
    public int GetMaxPlayers => ServerInfo.MaxPlayers; 
}
