using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : NetworkBehaviour {
    [Networked] private NetworkBool canStart { get; set; }
    [Networked] private NetworkBool hasStarted { get; set; }

    [Header("Time Syncing")]
    [SerializeField] private TMP_Text timerText;
    [Networked] private float Timer { get; set; }
    private static float sTimer;

    private bool playedCutscene;

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

        if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.STARTED) {
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