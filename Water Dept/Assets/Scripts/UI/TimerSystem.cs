using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystem : NetworkBehaviour {
    [Networked] private CustomTickTimer Timer { get; set; }
    [Networked] private NetworkBool canStart { get; set; }

    [Header("Time Syncing")]
    [Tooltip("Duration (Seconds)"), SerializeField] private float duration;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float remainingTime;

    private bool canPause = false, playedCutscene;
    private int prev, current;

    [SerializeField] private Button pauseButton, resetButton;

    private void Awake() {
        if (Application.isEditor) {
            pauseButton.gameObject.SetActive(true);
            resetButton.gameObject.SetActive(true);
        }
    }

    public override void Spawned() {
        remainingTime = duration;
        canStart = true;
        StartTimer();

    }

    public void StartTimer() {
        if (Runner.IsServer && canStart) {
            Timer = CustomTickTimer.CreateFromTicks(Runner, 999);
        }
    }

    public override void FixedUpdateNetwork() {
        Debug.Log($"Elapsed {Timer.ElapsedTicks(Runner)} ticks.");

        if (Timer.Expired(Runner)) {
            // Execute Logic

            // Reset timer
            Timer = default;

            Debug.Log("Timer Finished on tick: " + Runner.Tick);
        }


        /*if (GameStateHandler.Instance.GetGameState == GameStateHandler.GameState.STARTED) {
            if (!playedCutscene) {
                EventManager.TimerStart();
                playedCutscene = true;
            }
            if (!Timer.IsRunning) {
                //Timer = TickTimer.CreateFromSeconds(Runner, remainingTime);
            }
            if (canStart && !canPause) {
                if (Timer.Expired(Runner)) {
                    Debug.Log("Game Ended");
                    //Timer = TickTimer.None;
                    EventManager.TimerEnd();
                    canStart = false;
                }
            }
        }*/
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
        EventManager.onTimeChange -= DisplayTime;
    }
}