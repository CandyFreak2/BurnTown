using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DetectPauseMenu : MonoBehaviour {

    public GameObject pausePanel;

    private void OnEnable() {
        UIInputHandler.onActionPause += DetectPause;
    }

    private void OnDisable() {
        UIInputHandler.onActionPause -= DetectPause;
    }

    private void DetectPause() {
        if (pausePanel == null) {
            pausePanel = GameObject.FindWithTag("PauseMenu");
        }

        if (pausePanel != null) {
            EventSystem.current.firstSelectedGameObject = pausePanel.GetComponentInChildren<Button>(true).gameObject;
        }
    }
}
