using UnityEngine;

public static class ServerInfo {
    public const int MAX_PLAYER_COUNT = 4;

    public static string LobbyName;

    public static int MaxPlayers {
        get => PlayerPrefs.GetInt("S_MaxPlayers", 4);
        set => PlayerPrefs.SetInt("S_MaxPlayers", Mathf.Clamp(value, 1, MAX_PLAYER_COUNT));
    }
}
