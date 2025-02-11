using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialListener : MonoBehaviour {
    // Find a way for this script to listen for a list of UnityEvents and enables boolean that allows progression of tutorial
    //public static event Action OnPlayerEnterTruckRadius;

    public TutorialEvents tutorialEvents;
    public TextMeshProUGUI titleText, descriptionText;

    private int currentTutorial = 0;

    public UnityEvent OnPlayerMove, OnPlayerLook;

    private void Update() {
        titleText.text = tutorialEvents.tutorials[currentTutorial].title;
        descriptionText.text = tutorialEvents.tutorials[currentTutorial].description;

        // Testing Purposes
        if (Input.GetKeyUp(KeyCode.J) && currentTutorial != tutorialEvents.tutorials.Count - 1) {
            NextTutorial();
        }

        if (Input.GetKeyUp(KeyCode.K) && currentTutorial != 0) {
            PreviousTutorial();
        }
    }

    public void NextTutorial() { currentTutorial++; }
    public void PreviousTutorial() { currentTutorial--; }
}
