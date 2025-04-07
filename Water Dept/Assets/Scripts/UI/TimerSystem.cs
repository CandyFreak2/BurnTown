using Fusion;
using TMPro;
using UnityEngine;
using static GameStateHandler;

public class TimerSystem : NetworkBehaviour {
    [Networked] private NetworkBool canStart { get; set; }
    [Networked] private NetworkBool hasStarted { get; set; }

    [Header("Time Syncing")]
    [SerializeField] private TMP_Text timerText;
    [Networked] private float Timer { get; set; }
    private static float sTimer;

    private bool playedCutscene;
    private bool wasSaved = false;

    public override void Spawned() {
        canStart = true;
    }

    // Only used to update the timer's text if the timer is actually running
    public override void Render() {
        if (hasStarted) {
            RPC_SyncTimer();
        }
    }

    public void RPC_SyncTimer() {
        if (hasStarted) {
            DisplayTime(Timer);
        }
    }


    public override void FixedUpdateNetwork() {
        sTimer = Timer;

        if (Instance.GetGameState == GameState.STARTED) {
            if (!playedCutscene) {
                playedCutscene = true;
            }
            if (!hasStarted) {
                EventManager.TimerStart();
                hasStarted = true;
            }
            if (hasStarted) {
                Timer += Runner.DeltaTime;
            }
        }

        if (!wasSaved && Instance.GetGameState == GameState.END && Runner.IsServer) {
            PlayerPrefs.SetFloat("N_FinalTime", Timer);
            wasSaved = true;
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

    public static float GetTimerTime () => sTimer;
}