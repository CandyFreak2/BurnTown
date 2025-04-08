using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour {

    [Header("Join")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text charLength;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject loadScreen;
    private int charCount;
    private bool isLobbyValid = false;

    [SerializeField] private GameObject failureToJoinPanel;
    [SerializeField] private TMP_Text statusMessage, descriptionMessage;

    private void Start() {
        charCount = LobbyUI.GetCodeLength;
        codeInput.characterLimit = charCount;
    }

    private void Update() {
        if (codeInput.text.Length == charCount) {
            ClientInfo.LevelID = codeInput.text;
            confirmButton.interactable = true;
        } else if (codeInput.text.Length != charCount) {
            confirmButton.interactable = false;
        }
    }

    public void UpdateCharLength() {
        charLength.text = (charCount - codeInput.text.Length).ToString();
    }

    public void AddInput(string key) {
        if (codeInput.text.Length < charCount) {
            string newCode = codeInput.text + key;
            codeInput.text = newCode;
        }
    }

    public void ClearInput() {
        codeInput.text = string.Empty;
        HideFailureToJoinPanel();
    }
    
    public void DeleteInput() {
        if (codeInput.text.Length > 0) {
            string newCode = codeInput.text.Remove(codeInput.text.Length - 1);
            codeInput.text = newCode;
        }
        HideFailureToJoinPanel();
    }

    public void HideFailureToJoinPanel() {
        if (failureToJoinPanel.activeInHierarchy) {
            failureToJoinPanel.SetActive(false);
        }
    }

    public void FailedJoin(string status, string desc) {
        failureToJoinPanel.SetActive(true);
        statusMessage.text = status;
        descriptionMessage.text = desc; 
        loadScreen.SetActive(false);
    }

    public void ValidateLobby() {
        isLobbyValid = !string.IsNullOrEmpty(ClientInfo.LevelID);
    }

    public void TryCreateLobby(BasicSpawner launcher) {
        if (isLobbyValid) {
            ClientInfo.LevelID = codeInput.text;
            launcher.SetGameMode(GameMode.Client);
            launcher.JoinOrCreateLobby();
            isLobbyValid = false;
            loadScreen.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnEnable() {
        EventManager.onKeyPress += AddInput;
        EventManager.onVMClear += ClearInput;
        EventManager.onVMDelete += DeleteInput;
        EventManager.onFailedJoin += FailedJoin;
    }

    private void OnDisable() {
        EventManager.onKeyPress -= AddInput;
        EventManager.onVMClear -= ClearInput;
        EventManager.onFailedJoin -= FailedJoin;
    }
}
