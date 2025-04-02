using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : NetworkBehaviour {
    [Networked] private CustomTickTimer Timer { get; set; }
    [Networked] private NetworkBool canStart { get; set; }

    [Header("Time Syncing")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float remainingTime;
    private float duration = 0;

    private bool playedCutscene;
    private int prev, current;

    public override void Spawned() {
        remainingTime = duration;
        canStart = true;
    }

    // Only used to update the timer's text if the timer is actually running
    public override void Render() {
        if (Timer.IsRunning) {
            RPC_SyncTimer();
        }
    }

    public void RPC_SyncTimer() {
        if (Timer.IsRunning) {
            remainingTime = Timer.RemainingTime(Runner) ?? 0;
            DisplayTime(remainingTime);
        }
    }

    public override void FixedUpdateNetwork() {
        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.STARTED) {
            if (!playedCutscene) {
                EventManager.TimerStart();
                playedCutscene = true;
            }
            if (!Timer.IsRunning && canStart) {
                Timer = CustomTickTimer.CreateFromSeconds(Runner, 0);
            }
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
        if (timerText != null) {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            OnTimeChange(displayTime);
        }
    }

    public void OnTimeChange(float displayTime) {
        current = (int)displayTime;
        if (current < prev && Timer.IsRunning) {
            prev = current;
            EventManager.TimeChange(current);
        }
    }

    private void OnEnable() {
        EventManager.onTimeChange += OnTimeChange;
    }

    private void OnDisable() {
        EventManager.onTimeChange -= OnTimeChange;
    }
}