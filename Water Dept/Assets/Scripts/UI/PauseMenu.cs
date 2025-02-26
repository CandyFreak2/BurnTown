using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private GameObject pauseUI;

    public event Action<bool> OnPauseStateChanged;

    private bool isPanelOn = false;
    private NetworkRunner runner;

    private void Start() {
        runner = FindAnyObjectByType<NetworkRunner>();

        pauseUI.SetActive(isPanelOn);
        isPanelOn = pauseUI.activeInHierarchy;
    }

    public void TogglePause() {
        isPanelOn = !isPanelOn;
        pauseUI.SetActive(isPanelOn);

        OnPauseStateChanged?.Invoke(isPanelOn);
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
