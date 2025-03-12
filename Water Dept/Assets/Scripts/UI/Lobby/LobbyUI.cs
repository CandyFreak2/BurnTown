using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {
    [SerializeField] private TMP_Text lobbyCode;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private int codeLength = 4;

    //public Button confirmButton;

    private void Start() {
        playerCountSlider.SetValueWithoutNotify(4);
        SetLobbyCode(codeLength);
        SetPlayerCount();

        playerCountSlider.wholeNumbers = true;
        playerCountSlider.minValue = 1;
        playerCountSlider.maxValue = ServerInfo.MAX_PLAYER_COUNT;
        playerCountSlider.value = 2;
        playerCountSlider.onValueChanged.AddListener(x => ServerInfo.MaxPlayers = (int) x);
    
        ServerInfo.MaxPlayers = (int)playerCountSlider.value;
    }

    private void Update() {
        playerCountText.text = playerCountSlider.value.ToString();
    }

    // Generates a Random Lobby Name that spits out chars at the same rate without bias
    public void SetLobbyCode(int length) {
        System.Random r = new();
        StringBuilder result = new(length);

        for (int i = 0; i < length; i++) {
            var character = (char) r.Next('A', 'Z' + 11);
            if (character > 'Z') {
                character = (char)('0' + character - 1 - 'Z');
            }
            result.Append(character);
        }

        lobbyCode.text = result.ToString();
    }

    public void SetPlayerCount() {
        playerCountSlider.value = ServerInfo.MaxPlayers;
        playerCountText.text = $"{ServerInfo.MaxPlayers}";
    }


}
