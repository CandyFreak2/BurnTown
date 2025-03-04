using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : NetworkBehaviour, INetworkRunnerCallbacks {
    public float remainingTime;

    [Networked] private TickTimer Timer { get; set; }
    [Networked] private NetworkBool canStart { get; set; }
    private bool isSpawned = false;

    [Header("Time Syncing")]
    [Tooltip("Duration is based off of seconds"), SerializeField] private float duration;
    [SerializeField] private TMP_Text[] timerText;
    private GameObject[] timers;

    private bool isPaused = false;
    private bool isReset = false;

    private bool canPause = false;
    private bool canReset = false;

    private Button pauseButton, resetButton; 

    public override void Spawned() {
        // KEEP CODE
        remainingTime = duration;
        Timer = TickTimer.CreateFromSeconds(Runner, remainingTime);
        canStart = true;
        isSpawned = true;
    }

    public void FixedUpdate() {
        if (!isSpawned) return;

        timers = GameObject.FindGameObjectsWithTag("Clock");

        timerText = new TMP_Text[timers.Length];
        for (int i = 0; i < timers.Length; i++) {
            timerText[i] = timers[i].GetComponent<TMP_Text>();
        }

        // TEMP CODE REMOVE 
        pauseButton = GameObject.Find("Pause1").GetComponent<Button>();
        resetButton = GameObject.Find("Reset1").GetComponent<Button>();

        pauseButton.onClick.AddListener(PauseTimer);
        resetButton.onClick.AddListener(ResetTimer);
    }

    public void FixedUpdateNetworked() {
        if (canStart && !canPause) {
            if (Timer.Expired(Runner)) {
                Debug.Log("Game Ended");
                Timer = TickTimer.None;
                EventManager.TimerEnd();
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

    public void PauseTimer() {
        if (Timer.IsRunning) {
            Timer = TickTimer.None; 
            canPause = !canPause;
        }
    }

    public void ResetTimer() {
        if (canPause || !Timer.IsRunning) {
            duration = remainingTime;
            Timer = TickTimer.CreateFromSeconds(Runner, remainingTime); 
            canPause = false;
            canReset = true;
            canStart = true;
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