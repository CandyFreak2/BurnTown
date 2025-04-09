using System.Collections;
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

    [Header("Camera Manager Reference")]
    [SerializeField] CameraManager cameraManager;
    public GameObject buttonHost, buttonJoin, buttonPlay, buttonCredits, buttonQuit;

    private void Start() {
        eventSystem = EventSystem.current;
        GoBackToMainScreen();

        //turn of button visibility
        buttonHost.SetActive(false);
        buttonJoin.SetActive(false);

        //ugly patch fix to make the system work -James
        buttonPlay.transform.parent.gameObject.SetActive(false);
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
        //switch camera POV to main screen
        cameraManager.SwitchActiveCamera(cameraManager.startCam);

        //turn buttons off
        buttonHost.SetActive(false);
        buttonJoin.SetActive(false);

        //make main menu buttons interactable
        //disable interaction with buttons from main screen
        buttonPlay.GetComponent<Button>().interactable = true;
        buttonCredits.GetComponent<Button>().interactable = true;
        buttonQuit.GetComponent<Button>().interactable = true;

        ActivatePanel(startPanel, () => {
            if (hostButton == null) {
                hostButton = buttonPlay;
            }
            return hostButton;  
        }); 
    }

    private IEnumerator SetSelectedAfterDelay(System.Action action) {
        yield return null;
        action.Invoke();
    }

    public void LobbyCamSwitch()
    {
        cameraManager.SwitchActiveCamera(cameraManager.lobbyCam);
        buttonHost.SetActive(true);
        buttonJoin.SetActive(true);
        EventSystem.current.SetSelectedGameObject(buttonHost);

        //disable interaction with buttons from main screen
        buttonPlay.GetComponent<Button>().interactable = false;
        buttonCredits.GetComponent<Button>().interactable = false;
        buttonQuit.GetComponent<Button>().interactable = false;
    }
}
