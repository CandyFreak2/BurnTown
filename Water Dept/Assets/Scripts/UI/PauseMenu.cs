using Fusion;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : NetworkBehaviour {
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NetworkRunner runner;

    private bool isPanelOn = false;
    private bool isSpawned = false;

    private void OnEnable() {
        UIInputHandler.onActionPause += TogglePause;
    }

    private void OnDisable() {
        UIInputHandler.onActionPause -= TogglePause;
    }

    public override void Spawned() {
        isSpawned = true;
    }

    private void FixedUpdate() {
        if (isSpawned && pauseUI == null && runner == null) {
            runner = FindAnyObjectByType<NetworkRunner>();
            pauseUI = UIManager.Instance.GetPauseMenu();

            pauseUI.SetActive(isPanelOn);
            isPanelOn = pauseUI.activeInHierarchy;
        }
    }

    public void TogglePause(bool input) {
        input = !input;
        pauseUI.SetActive(input);

        if (input) {
            Cursor.lockState = CursorLockMode.Locked;
        }

        RPC_SetInputState(input);
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetInputState(bool enable, RpcInfo info = default) {
        playerMovement = runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
        playerMovement.enabled = !enable;
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
