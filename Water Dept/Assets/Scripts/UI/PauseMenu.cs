using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private GameObject pauseUI;

    private bool isPanelOn = false;
    private NetworkRunner runner;

    private void Start() {
        runner = FindAnyObjectByType<NetworkRunner>();

        pauseUI.SetActive(isPanelOn);
        isPanelOn = pauseUI.activeInHierarchy;
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isPanelOn = pauseUI.activeInHierarchy;
            pauseUI.SetActive(!isPanelOn);
        }
    }

    public void Resume() {
        isPanelOn = false;
        pauseUI.SetActive(isPanelOn);
    }

    public void Reset() {
        runner.LoadScene(SceneManager.GetActiveScene().name);
        //runner.
    }

    public void Disconnect() {
        //runner.Disconnect(localPlayer);
    }

    public void Quit() {
        Application.Quit();
    }
}
