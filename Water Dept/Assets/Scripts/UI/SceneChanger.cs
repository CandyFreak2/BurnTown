using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    [SerializeField] private string tutorialScene, playScene;
   
    public void PlayTutorial() {
        SceneManager.LoadScene(tutorialScene);
    }

    public void PlayLobby() {
        SceneManager.LoadScene(playScene);
    }
}
