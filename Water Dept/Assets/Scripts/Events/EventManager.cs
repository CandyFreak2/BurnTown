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

    public static event UnityAction onEndZoneEnter;
    public static void EndZoneEnter() => onEndZoneEnter?.Invoke();

    public static event UnityAction onEndZoneExit;
    public static void EndZoneExit() => onEndZoneExit?.Invoke();

    public static event UnityAction onEndZoneTrigger;
    public static void EndZoneTrigger() => onEndZoneTrigger?.Invoke();

    // UI Lobby System
    public static event UnityAction<string> onKeyPress;
    public static void KeyboardPress(string key) => onKeyPress.Invoke(key);

    public static event UnityAction onVMClear;
    public static void VMClear() => onVMClear.Invoke();

    public static event UnityAction onVMDelete;
    public static void VMDelete() => onVMDelete.Invoke();
}
