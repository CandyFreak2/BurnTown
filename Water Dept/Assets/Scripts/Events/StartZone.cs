using Fusion;
using UnityEngine;
using System.Collections.Generic;
using static GameStateHandler;
using System.Collections;
using UnityEngine.Playables;

public class StartZone : NetworkBehaviour {
    private NetworkBool isSpawned { get; set; }
    [SerializeField] private List<GameObject> players = new();

    [Header("Timer")]
    [Networked] public TickTimer Timer { get; set; }
    [Tooltip("Duration (Seconds)"), SerializeField] private float duration;
    [Networked] private int playersInRadius { get; set; }
    [Networked] private int maxPlayers { get; set; }
    [Networked] private string lobbyCode { get; set; }

    public override void Spawned() {
        isSpawned = true;
        if (HasStateAuthority) {
            maxPlayers = ServerInfo.MaxPlayers;
            lobbyCode = ServerInfo.LobbyName;
        }
    }

    public override void FixedUpdateNetwork() {
        if (Instance.GetGameState == GameState.NOT_STARTED) {
            if (playersInRadius >= maxPlayers && IsAllTrue(players)) {
                RPC_StartGame();
            }
        }

        if (isSpawned) {
            if (playersInRadius >= maxPlayers && !Timer.IsRunning) {
                Timer = TickTimer.CreateFromSeconds(Runner, duration);
                EventManager.StartZoneTimerStart();
            }

            if (playersInRadius < maxPlayers && Timer.IsRunning) {
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
        if (isSpawned && other.CompareTag("Player")) {
            RPC_PlayerEnterStart(Object.Id, other.transform.parent.GetComponent<PlayerMovement>().Object.Id);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (isSpawned && other.CompareTag("Player")) {
            RPC_PlayerExitStart(Object.Id);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayerEnterStart(NetworkId _startZoneId, NetworkId _otherId) {
        StartZone startZone = Runner.FindObject(_startZoneId).GetComponent<StartZone>();
        PlayerMovement playerMovement = Runner.FindObject(_otherId).GetComponent<PlayerMovement>();

        if (startZone != null && playerMovement != null) {
            if (HasStateAuthority) {
                startZone.playersInRadius++;
            }
            if (!startZone.players.Contains(playerMovement.gameObject) && Instance.GetGameState == GameState.NOT_STARTED) {
                startZone.players.Add(playerMovement.gameObject);
                playerMovement.IsReady = true;
                EventManager.StartZoneEnter();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayerExitStart(NetworkId zoneID) {
        if (isSpawned) {
            StartZone startZone = Runner.FindObject(zoneID).GetComponent<StartZone>();

            if (startZone != null && HasStateAuthority) {
                startZone.playersInRadius--;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartGame() {
        Instance.SetGameState(GameState.CUTSCENE);
        EventManager.StartZoneTrigger();
        StartCoroutine(DelayStartGameState(9.5f));
    }

    IEnumerator DelayStartGameState(float delay) {
        yield return new WaitForSeconds(delay);
        Instance.SetGameState(GameState.STARTED);
    }

    public float GetTimeLeft => Timer.RemainingTime(Runner) ?? duration;
    public int GetCurrentPlayers => playersInRadius;
    public int GetMaxPlayers => maxPlayers;
}
