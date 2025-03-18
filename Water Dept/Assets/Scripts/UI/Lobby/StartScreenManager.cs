using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class StartScreenManager : MonoBehaviour {
    [Header("Host")]


    [Header("Join")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text charLength;
    [SerializeField] private Button confirmButton;
    private int charCount;

    private bool isLobbyValid = false;

    //[Header("Collective")]

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

    public void ValidateLobby() {
        isLobbyValid = !string.IsNullOrEmpty(ClientInfo.LevelID);
    }

    public void TryCreateLobby(BasicSpawner launcher) {
        if (isLobbyValid) {
            ClientInfo.LevelID = codeInput.text;
            Debug.Log("Joining lobby");
            launcher.SetGameMode(GameMode.Client);
            launcher.JoinOrCreateLobby();
            isLobbyValid = false;
        }
    }
}
