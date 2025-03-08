using UnityEngine.Events;

public static class EventManager {
    public static void TimerStart() => onTimerStart?.Invoke();
    public static event UnityAction onTimerStart;

    public static void TimerEnd() => onTimerEnd?.Invoke();
    public static event UnityAction onTimerEnd;

    public static void BurnableDestroyed(int score) => onBurnableDestroyed?.Invoke(score);
    public static event UnityAction<int> onBurnableDestroyed;

}
