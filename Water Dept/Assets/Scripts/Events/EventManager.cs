using UnityEngine.Events;

public static class EventManager {
    public static event UnityAction onTimerStart;
    public static void TimerStart() => onTimerStart?.Invoke();

    public static event UnityAction onTimerEnd;
    public static void TimerEnd() => onTimerEnd?.Invoke();

    public static event UnityAction<int> OnBurnableDestroyed;
    public static void BurnableDestroyed(int score) => OnBurnableDestroyed?.Invoke(score);

}
