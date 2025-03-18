using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour {
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject joinPanel;

    [SerializeField] private LobbyUI codeGenerator;
    private EventSystem eventSystem;
    private GameObject hostStart;
    private GameObject joinStart;

    private void Start() {
        eventSystem = EventSystem.current;
        GoBackToMainScreen();
    }

    private void Update() {
        if (hostPanel.activeInHierarchy) {
            if (hostStart == null) {
                hostStart = FindAnyObjectByType<Slider>().gameObject;
            }
            eventSystem.firstSelectedGameObject = hostStart;
        }

        if (joinPanel.activeInHierarchy) {
            if (joinStart == null) {
                joinStart = FindAnyObjectByType<TMP_InputField>().gameObject;
            }
            eventSystem.firstSelectedGameObject = joinStart;
        }
    }

    public void GoToHostPanel() {
        startPanel.SetActive(false);
        codeGenerator.enabled = true;
        hostPanel.SetActive(true);
    }

    public void GoToJoinPanel() {
        startPanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void GoBackToMainScreen() {
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);
        startPanel.SetActive(true);
    }
}
