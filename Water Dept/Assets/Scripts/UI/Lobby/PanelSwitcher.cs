using System.Collections;
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
    private GameObject hostButton, hostStart, joinStart;

    private void Start() {
        eventSystem = EventSystem.current;
        GoBackToMainScreen();
    }

    private void ActivatePanel(GameObject panelToActivate, System.Func<GameObject> GetStartElement) {
        startPanel.SetActive(false);
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);

        panelToActivate.SetActive(true);

        StartCoroutine(SetSelectedAfterDelay(() => {
            GameObject startElement = GetStartElement?.Invoke();
            if (startElement != null) {
                eventSystem.SetSelectedGameObject(startElement);
            }
        }));
    }

    public void GoToHostPanel() {
        ActivatePanel(hostPanel, () => {
            if (hostStart == null) {
                hostStart = FindAnyObjectByType<Slider>().gameObject;
            }
            return hostStart;
        });

        codeGenerator.enabled = true; 
    }

    public void GoToJoinPanel() {
        ActivatePanel(joinPanel, () => {
            if (joinStart == null) {
                joinStart = GameObject.Find("1");
            }
            return joinStart;
        });
    }

    public void GoBackToMainScreen() {
        ActivatePanel(startPanel, () => {
            if (hostButton == null) {
                hostButton = FindFirstObjectByType<Button>().gameObject;
            }
            return hostButton;  
        }); 
    }

    private IEnumerator SetSelectedAfterDelay(System.Action action) {
        yield return null;
        action.Invoke();
    }

}
