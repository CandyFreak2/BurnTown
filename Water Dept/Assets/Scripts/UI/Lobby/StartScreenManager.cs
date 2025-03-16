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

    private bool isLobbyValid;

    //[Header("Collective")]

    private void Start() {
        charCount = LobbyUI.GetCodeLength;
        codeInput.characterLimit = charCount;
    }

    private void Update() {
        if (codeInput.text.Length == charCount) {
            confirmButton.interactable = true;
        }
        confirmButton.interactable = false;
    }

    public void UpdateCharLength() {
        charLength.text = (charCount - codeInput.text.Length).ToString();
    }

    public void ValidateLobby() {
        isLobbyValid = !ClientInfo.LevelID.IsNullOrEmpty();
    }

    public void TryCreateLobby(BasicSpawner launcher) {
        if (isLobbyValid) {
            ClientInfo.LevelID = codeInput.text;
            launcher.SetGameMode(GameMode.Client);
            launcher.JoinOrCreateLobby();
            isLobbyValid = false;
        }
    }
}
