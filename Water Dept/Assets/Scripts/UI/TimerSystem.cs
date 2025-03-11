using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : NetworkBehaviour {
    [Networked] private TickTimer Timer { get; set; }
    [Networked] private NetworkBool canStart { get; set; }

    [Header("Time Syncing")]
    [Tooltip("Duration (Seconds)"), SerializeField] private float duration;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float remainingTime;

    private bool canPause = false;

    [SerializeField] private Button pauseButton, resetButton; 

    private void Awake() {
        if (Application.isEditor) {
            pauseButton.gameObject.SetActive(true);
            resetButton.gameObject.SetActive(true);
        }
    }

    public override void Spawned() {
        remainingTime = duration;
        Timer = TickTimer.CreateFromSeconds(Runner, remainingTime);
        canStart = true;
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
        if (timerText != null) {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
            canStart = true;
        }
    }

    public void AddTime(float increment) {
        if (Timer.IsRunning) {
            Timer = TickTimer.None;
            remainingTime += increment;
            Timer = TickTimer.CreateFromSeconds(Runner, remainingTime);
        }
    }

    private void OnEnable() {
        EventManager.onTimerAdd += AddTime;
    }

    private void OnDisable() {
        EventManager.onTimerAdd -= AddTime;
    }
}