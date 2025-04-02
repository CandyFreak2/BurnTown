using UnityEngine;
using UnityEngine.Playables;

public class RunCutscene : MonoBehaviour
{
    private void OnEnable() {
        EventManager.onPlayerJoin += Run;
    }

    private void OnDisable() {
        EventManager.onPlayerJoin -= Run;
    }

    public void Run() {
        GetComponent<PlayableDirector>().Play();
    }
}
