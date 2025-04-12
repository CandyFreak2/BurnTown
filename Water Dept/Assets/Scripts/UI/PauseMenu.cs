using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static GameStateHandler;

public class PauseMenu : NetworkBehaviour {
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private GameObject startButton;
    [SerializeField] private TMP_Text lobbyCode;

    private bool isPanelOn = false;
    private bool isSpawned = false;
    private NetworkObject playerObject;

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
            lobbyCode.text = ServerInfo.LobbyName;

            pauseUI.SetActive(isPanelOn);
            isPanelOn = pauseUI.activeInHierarchy;
        }
    }

    public void TogglePause(bool input) {
        input = !input;
        pauseUI.SetActive(input);
        EventSystem.current.SetSelectedGameObject(startButton);

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

    public void ResetPlayer() {
        RPC_GetPlayer();
        RPC_RespawnPlayer(playerObject.Id);
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_GetPlayer(RpcInfo info = default) {
        playerObject = runner.GetPlayerObject(info.Source).GetComponent<NetworkObject>();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RespawnPlayer(NetworkId _otherId) {
        NetworkObject playerNetworkObj = Runner.FindObject(_otherId);

        SimpleKCC playerKCC = playerNetworkObj.GetComponent<SimpleKCC>();
        if (playerKCC != null) {
            playerKCC.SetPosition(ObjectiveIndicatorList.Singleton.GetLastObjectivePos() + Vector3.up);
        }
    }

    public void Disconnect() {
        if (runner.IsClient) {
            SceneManager.LoadScene(0);
            runner.Shutdown();
            Instance.SetGameState(GameState.NOT_STARTED);
        }

        if (runner.IsServer) {
            runner.UnloadScene(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
            SceneManager.LoadScene(0);
            runner.Shutdown();
            Instance.SetGameState(GameState.NOT_STARTED);
        }
    }

    public void Quit() {
        Application.Quit();
    }
}
