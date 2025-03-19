using System.Collections;
using Fusion;
using UnityEngine;

public class LevelManager : NetworkSceneManagerDefault {
    public const int START_SCENE = 0;
    public const int LOBBY_SCENE = 1;

    public static LevelManager Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            }
            else if (singleton == null) {
                singleton = value;
            }
            else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(LevelManager)}!");
            }
        }
    }
    private static LevelManager singleton;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadLevel(int index) {
        Instance.Runner.LoadScene(SceneRef.FromIndex(index));
    }

    protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams) {
        Debug.Log($"Loading scene {sceneRef}");

        yield return base.LoadSceneCoroutine(sceneRef, sceneParams);

        // Delay one frame, ensuring objects are spawned locally
        yield return null;

        // Now we can safely spawn karts
        if (/*GameManager.CurrentTrack != null &&*/ sceneRef.AsIndex > START_SCENE) {
            if (Runner.GameMode == GameMode.Host) {
                foreach (var player in Firefighter.players) {
                    player.GameState = Firefighter.PlayerState.CUTSCENE;
                    Debug.Log("Spawn Player");
                    //SpawnPlayer(Runner, player);
                }
            }
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }
}
