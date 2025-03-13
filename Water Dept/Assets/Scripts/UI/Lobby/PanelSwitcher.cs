using UnityEngine;

public class PanelSwitcher : MonoBehaviour {
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject joinPanel;

    private void Start() {
        GoBackToMainScreen();
    }

    public void GoToHostPanel() {
        startPanel.SetActive(false);
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
