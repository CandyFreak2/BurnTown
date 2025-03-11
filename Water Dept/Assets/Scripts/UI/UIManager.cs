using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            } else if (singleton == null) {
                singleton = value;
            } else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}!");
            }
        }
    }
    private static UIManager singleton;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject pauseMenu;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void SetUI() {
        scoreText.enabled = true; 
        timerText.enabled = true;
        pauseMenu.SetActive(false);
    }

    public GameObject GetPauseMenu() => pauseMenu;
}
