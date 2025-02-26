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
        if (Input.GetKeyDown(KeyCode.Escape) && !pauseUI.activeInHierarchy) {
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
        runner.Shutdown();
    }

    public void Disconnect() {
        if (runner.IsClient) {
            SceneManager.LoadScene(0);
            runner.Shutdown();
        }

        if (runner.IsServer) {
            runner.UnloadScene(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
            SceneManager.LoadScene(0);
            runner.Shutdown();
        }
    }

    public void Quit() {
        Application.Quit();
    }
}
