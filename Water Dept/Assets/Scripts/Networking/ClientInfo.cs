using UnityEngine;

public static class ClientInfo {
    public static string LevelID {
        get => PlayerPrefs.GetString("C_LevelID", "");
        set => PlayerPrefs.SetString("C_LevelID", value);
    }
}
