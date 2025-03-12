using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {
    public TMP_Text lobbyName;
    public Slider playerCountSlider;
    public TMP_Text playerCountText;

    public int lobbyNameLength = 4;

    //public Button confirmButton;

    private void Start() {
        playerCountSlider.SetValueWithoutNotify(4);
        SetLobbyName(lobbyNameLength);
        SetPlayerCount();

        playerCountSlider.wholeNumbers = true;
        playerCountSlider.minValue = 1;
        playerCountSlider.maxValue = ServerInfo.MAX_PLAYER_COUNT;
        playerCountSlider.value = 2;
        playerCountSlider.onValueChanged.AddListener(x => ServerInfo.MaxPlayers = (int) x);
    
        ServerInfo.MaxPlayers = (int)playerCountSlider.value;
    }

    // Creates a Random Lobby Name 
    public void SetLobbyName(int length) {
        System.Random r = new();
        StringBuilder result = new(length);

        for (int i = 0; i < length; i++) {
            int choice = r.Next(0, 3);
            var character = choice switch {
                0 => (char) r.Next('A', 'Z' + 1),
                1 => (char) r.Next('a', 'z' + 1),
                2 => (char) r.Next('0', '9' + 1),
                _ => throw new IndexOutOfRangeException("Random went out of length range"),
            };
            result.Append(character);
        }

        lobbyName.text = result.ToString();
    }

    public void SetPlayerCount() {
        playerCountSlider.value = ServerInfo.MaxPlayers;
        playerCountText.text = $"{ServerInfo.MaxPlayers}";
    }


}
