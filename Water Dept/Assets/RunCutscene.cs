using UnityEngine;
using UnityEngine.Playables;

public class RunCutscene : MonoBehaviour
{
    private void OnEnable() {
        EventManager.onTimerStart += Run;
    }

    private void OnDisable() {
        EventManager.onTimerStart -= Run;
    }

    public void Run() {
        GetComponent<PlayableDirector>().Play();
    }
}
