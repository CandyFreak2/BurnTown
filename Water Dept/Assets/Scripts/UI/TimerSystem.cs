using Fusion;
using TMPro;
using UnityEngine;

public class TimerSystem : NetworkBehaviour {
    [Networked] private TickTimer Timer { get; set; }

    [Header("Time Syncing")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float duration;

    public override void Spawned() {
        Timer = TickTimer.CreateFromSeconds(Runner, duration);
    }

    public override void FixedUpdateNetwork() {
        if (Timer.Expired(Runner)) {
            Debug.Log("Game Ended");
            Timer = TickTimer.None;
        }
    }

    // Only used to update the timer's text if the timer is actually running
    public override void Render() {
        if (Timer.IsRunning) {
            DisplayTime(Timer.RemainingTime(Runner) ?? 0);
        }
    }

    public void DisplayTime(float displayTime) { 
        if (displayTime < 0) {
            displayTime = 0;
        }
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(displayTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
