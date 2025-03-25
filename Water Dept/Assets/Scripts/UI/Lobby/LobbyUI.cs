using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class LobbyUI : MonoBehaviour {
    [SerializeField] private TMP_Text lobbyCode;
    private static TMP_Text sLobbyCode;

    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private static int codeLength = 4;

    [SerializeField] private TMP_Dropdown level;
    [SerializeField] private Image levelImage;
    private static TMP_Dropdown sLevel;

    public Button confirmButton;
    private bool isLobbyValid;

    private void Start() {
        sLobbyCode = lobbyCode;

        playerCountSlider.SetValueWithoutNotify(4);
        SetLobbyCode(codeLength);
        SetPlayerCount();

        level.AddOptions(ResourceManager.Instance.levels.Select(x => x.levelName).ToList());
        level.onValueChanged.AddListener(SetLevel);
        SetLevel(0);

        playerCountSlider.wholeNumbers = true;
        playerCountSlider.minValue = 1;
        playerCountSlider.maxValue = ServerInfo.MAX_PLAYER_COUNT;
        playerCountSlider.value = 2;
        playerCountSlider.onValueChanged.AddListener(x => ServerInfo.MaxPlayers = (int)x);

        ServerInfo.MaxPlayers = (int)playerCountSlider.value;
        ServerInfo.LobbyName = lobbyCode.text.ToString();
        ServerInfo.LevelID = level.value;
    }

    private void Update() {
        sLevel = level;
        playerCountText.text = playerCountSlider.value.ToString();
        ServerInfo.MaxPlayers = (int)playerCountSlider.value;
        ServerInfo.LevelID = level.value;
    }

    // Generates a Random Lobby Name that spits out chars at the same rate without bias
    public void SetLobbyCode(int length) {
        System.Random r = new();
        StringBuilder result = new(length);

        for (int i = 0; i < length; i++) {
            var character = (char)r.Next('A', 'Z' + 11);
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

    public void SetLevel(int id) {
        ServerInfo.LevelID = id;
        levelImage.sprite = ResourceManager.Instance.levels[id].levelIcon;
    }

    public static int GetCodeLength => codeLength;
    public static string GetSessionCode => sLobbyCode.text;

    public static int GetScene => ResourceManager.Instance.levels[sLevel.value].buildIndex;

    public void ValidateLobby() {
        isLobbyValid = !string.IsNullOrEmpty(ServerInfo.LobbyName);
    }

    public void TryCreateLobby(BasicSpawner launcher) {
        if (isLobbyValid) {
            launcher.SetGameMode(GameMode.Host);
            launcher.JoinOrCreateLobby();
            isLobbyValid = false;
        }
    }
}
