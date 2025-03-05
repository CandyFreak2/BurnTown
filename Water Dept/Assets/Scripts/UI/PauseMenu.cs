using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private PlayerMovement playerMovement;

    private bool isPanelOn = false;
    private NetworkRunner runner;

    private void OnEnable() {
        UIInputHandler.onActionPause += TogglePause;
    }

    private void OnDisable() {
        UIInputHandler.onActionPause -= TogglePause;
    }

    private void Start() {
        runner = FindAnyObjectByType<NetworkRunner>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        pauseUI = UIManager.Instance.GetPauseMenu();

        pauseUI.SetActive(isPanelOn);
        isPanelOn = pauseUI.activeInHierarchy;
    }

    public void TogglePause(bool input) {
        input = !input;
        pauseUI.SetActive(input);

        if (input) {
            Cursor.lockState = CursorLockMode.Locked;
        }

        playerMovement.enabled = !input;
    }

    public void Resume() {
        isPanelOn = false;
        pauseUI.SetActive(isPanelOn);
        playerMovement.enabled = true;
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
