using UnityEngine;

public class GameStateHandler : MonoBehaviour {
    public enum GameState {
        NOT_STARTED,
        CUTSCENE,
        STARTED,
        END
    }
    public static GameStateHandler Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            } else if (singleton == null) {
                singleton = value;
            } else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(GameStateHandler)}!");
            }
        }
    }
   
    public static GameStateHandler singleton;
    [SerializeField] private GameState state = GameState.NOT_STARTED;

    public GameState GetGameState => state;
    public void SetGameState(GameState nState) => state = nState;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}