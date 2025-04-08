using UnityEngine.Events;

public static class EventManager {
    public static void PlayerJoin() => onPlayerJoin?.Invoke();
    public static event UnityAction onPlayerJoin;

    public static void PlayerLeave() => onPlayerLeave?.Invoke();
    public static event UnityAction onPlayerLeave;

    public static void TimerStart() => onTimerStart?.Invoke();
    public static event UnityAction onTimerStart;

    public static void TimerEnd() => onTimerEnd?.Invoke();
    public static event UnityAction onTimerEnd;

    public static event UnityAction<float> onTimerAdd;
    public static void TimerAdd(float increment) => onTimerAdd?.Invoke(increment);

    public static event UnityAction<float> onTimeChange;
    public static void TimeChange(float currentTime) => onTimeChange?.Invoke(currentTime);

    public static void BurnableDestroyed(int score) => onBurnableDestroyed?.Invoke(score);
    public static event UnityAction<int> onBurnableDestroyed;

    public static void PlayerStunned(bool isStunned) => onPlayerStunned?.Invoke(isStunned);
    public static event UnityAction<bool> onPlayerStunned;

    // State Manager Triggers
    public static event UnityAction onStartZoneEnter;
    public static void StartZoneEnter() => onStartZoneEnter?.Invoke();

    public static event UnityAction onStartZoneExit;
    public static void StartZoneExit() => onStartZoneExit?.Invoke();

    public static event UnityAction onStartZoneTrigger;
    public static void StartZoneTrigger() => onStartZoneTrigger?.Invoke();

    public static event UnityAction onStartZoneTimerStart;
    public static void StartZoneTimerStart() => onStartZoneTimerStart?.Invoke();

    public static event UnityAction onStartZoneTimerReset;
    public static void StartZoneTimerReset() => onStartZoneTimerReset?.Invoke();

    public static event UnityAction<EndConditions> onEndZoneEnter;
    public static void EndZoneEnter(EndConditions condition) => onEndZoneEnter?.Invoke(condition);

    public static event UnityAction<EndConditions> onEndZoneExit;
    public static void EndZoneExit(EndConditions condition) => onEndZoneExit?.Invoke(condition);

    public static event UnityAction<EndConditions> onEndZoneTrigger;
    public static void EndZoneTrigger(EndConditions condition) => onEndZoneTrigger?.Invoke(condition);

    // UI Lobby System
    public static event UnityAction<string> onKeyPress;
    public static void KeyboardPress(string key) => onKeyPress.Invoke(key);

    public static event UnityAction onVMClear;
    public static void VMClear() => onVMClear.Invoke();

    public static event UnityAction onVMDelete;
    public static void VMDelete() => onVMDelete.Invoke();

    public static event UnityAction<string, string> onFailedJoin;
    public static void FailedJoin(string cause, string description) => onFailedJoin?.Invoke(cause, description);
}
