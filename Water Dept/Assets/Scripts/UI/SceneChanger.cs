using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    [SerializeField] private SceneAsset tutorialScene, lobbyScene;
   
    public void PlayTutorial() {
        SceneManager.LoadScene(tutorialScene.name);
    }

    public void PlayLobby() {
        SceneManager.LoadScene(lobbyScene.name);
    }
}
