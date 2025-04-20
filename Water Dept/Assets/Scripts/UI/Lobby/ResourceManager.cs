using UnityEngine;

public class ResourceManager : MonoBehaviour {
   
    public static ResourceManager Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            } else if (singleton == null) {
                singleton = value;
            } else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(ResourceManager)}!");
            }
        }
    }
    private static ResourceManager singleton;

    public LevelDefinitions[] levels;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.visible = focus;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
