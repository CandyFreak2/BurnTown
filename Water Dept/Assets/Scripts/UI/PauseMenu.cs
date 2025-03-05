using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private GameObject pauseUI;

    public event Action<bool> OnPauseStateChanged;

    private bool isPanelOn = false;
    private NetworkRunner runner;

    [SerializeField] private PlayerMovement playerMovement;

    private void OnEnable() {
        UIInputHandler.onActionPause += TogglePause;
    }

    private void OnDisable() {
        UIInputHandler.onActionPause -= TogglePause;
    }

    private void Start() {
        runner = FindAnyObjectByType<NetworkRunner>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        pauseUI = GameObject.FindGameObjectWithTag("PauseMenu").transform.GetChild(0).gameObject;

        pauseUI.SetActive(isPanelOn);
        isPanelOn = pauseUI.activeInHierarchy;
    }

    public void TogglePause() {
        isPanelOn = !isPanelOn;
        pauseUI.SetActive(isPanelOn);
        Debug.Log("Panel is on: " +  isPanelOn);

        if (isPanelOn) {
            Cursor.lockState = CursorLockMode.Locked;
        }

        OnPauseStateChanged?.Invoke(isPanelOn);
        playerMovement.enabled = !isPanelOn;
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
