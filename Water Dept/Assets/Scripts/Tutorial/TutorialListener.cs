using TMPro;
using UnityEngine;

public class TutorialListener : MonoBehaviour {
    // Find a way for this script to listen for a list of UnityEvents and enables boolean that allows progression of tutorial
    //public static event Action OnPlayerEnterTruckRadius;

    public TutorialEvents tutorialEvents;
    public TextMeshProUGUI titleText, descriptionText;
    public TPlayerInputHandeler handler;

    private int currentTutorial = 0;
    private bool hasMovedUp, hasMovedDown, hasMovedLeft, hasMovedRight;

    private void Update() {
        titleText.text = tutorialEvents.tutorials[currentTutorial].title;
        descriptionText.text = tutorialEvents.tutorials[currentTutorial].description;

        HandleTutorial();
    }

    private void HandleTutorial() {
        if (TryProgressTutorial(1, ref hasMovedUp, handler.IsPlayerMovingUp)) return;
        if (TryProgressTutorial(2, ref hasMovedDown, handler.IsPlayerMovingDown, hasMovedUp)) return;
        if (TryProgressTutorial(3, ref hasMovedLeft, handler.IsPlayerMovingLeft, hasMovedUp, hasMovedDown)) return;

        // Last tutorial should always be without nextTutorial parameter
        if (TryProgressTutorial(ref hasMovedRight, handler.IsPlayerMovingRight, hasMovedUp, hasMovedDown, hasMovedLeft)) return;
    }

    // All parameters after isMoving will take in as an array
    private bool TryProgressTutorial(ref bool movementFlag, bool isMoving, params bool[] prerequisites) {
        if (movementFlag || !isMoving) return false;

        foreach (bool prerequisite in prerequisites) {
            if (!prerequisite) return false;
        }

        NextTutorial();
        movementFlag = true;
        return true;
    }

    private bool TryProgressTutorial(int nextTutorial, ref bool movementFlag, bool isMoving, params bool[] prerequisites) {
        if (movementFlag || !isMoving) return false;

        foreach (bool prerequisite in prerequisites) {
            if (!prerequisite) return false;
        }

        NextTutorial(nextTutorial);
        movementFlag = true;
        return true;
    }

    public void NextTutorial() { 
        if (currentTutorial != tutorialEvents.tutorials.Count - 1) {
            currentTutorial++;
        }
    }

    public void NextTutorial(int newTutorial) {
        currentTutorial = newTutorial;
    }
}
