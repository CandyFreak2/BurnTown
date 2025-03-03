using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerSystem : NetworkBehaviour, INetworkRunnerCallbacks {
    [Networked] private TickTimer Timer { get; set; }
    [Networked] private NetworkBool canStart { get; set; }

    [Header("Time Syncing")]
    [Tooltip("Duration is based off of seconds"), SerializeField] private float duration;
    private TMP_Text[] timerText;
    private GameObject[] timers;

    [Header("Designer Controls")]
    public float remainingTime;
    public bool isPaused = false;
    public bool isReset = false;
    

    public override void Spawned() {
        remainingTime = duration;
        Timer = TickTimer.CreateFromSeconds(Runner, remainingTime);
        canStart = true;
    }

    public void FixedUpdate() {
        timers = GameObject.FindGameObjectsWithTag("Clock");

        timerText = new TMP_Text[timers.Length];
        for (int i = 0; i < timers.Length; i++) {
             timerText[i] = timers[i].GetComponent<TMP_Text>();
        }
    }

    public void FixedUpdateNetworked() {
        if (canStart) {
            if (Timer.Expired(Runner)) {
                Debug.Log("Game Ended");
                Timer = TickTimer.None;
                EventManager.OnTimerEnd?.Invoke();
                canStart = false; 
            }
        }
    }

    // Only used to update the timer's text if the timer is actually running
    public override void Render() {
        if (Timer.IsRunning) {
            RPC_SyncTimer();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncTimer() {
        if (Timer.IsRunning) {
            remainingTime = Timer.RemainingTime(Runner) ?? 0;
            DisplayTime(remainingTime);
        }
    }

    // Update the timer text across all players' UI
    public void DisplayTime(float displayTime) {
        if (displayTime < 0) {
            displayTime = 0;
        }
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(displayTime % 60);

        // Update all player's UI timer text
        foreach (TMP_Text timer in timerText) {
            if (timer != null) {
                timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    // This function gets triggered when a player joins to allow re-syncing
    public void UpdateTimerReferences() {
        timers = GameObject.FindGameObjectsWithTag("Clock");

        timerText = new TMP_Text[timers.Length];
        for (int i = 0; i < timers.Length; i++) {
            timerText[i] = timers[i].GetComponent<TMP_Text>();
        }
    }

    #region NETWORK_STUFF
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        UpdateTimerReferences();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        UpdateTimerReferences();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner) {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner) {
        throw new NotImplementedException();
    }
    #endregion
}