using NUnit.Framework;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TutorialListener : MonoBehaviour {
    // Find a way for this script to listen for a list of UnityEvents and enables boolean that allows progression of tutorial
    //public static event Action OnPlayerEnterTruckRadius;

    public TutorialEvents tutorialEvents;
    public TextMeshProUGUI titleText, descriptionText;

    private int currentTutorial = 0;

    public UnityEvent OnPlayerMove, OnPlayerLook;

    private void Awake() {
        titleText.text = tutorialEvents.tutorials[currentTutorial].title;
        descriptionText.text = tutorialEvents.tutorials[currentTutorial].description;
    }

    //private void Update() {
    //    OnPlayerMove?.Invoke();
    //}

}
