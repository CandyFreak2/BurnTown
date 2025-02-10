using System;
using UnityEngine;
using UnityEngine.Events;

public class TutorialListener : MonoBehaviour {
    // Find a way for this script to listen for a list of UnityEvents and enables boolean that allows progression of tutorial
    //public static event Action OnPlayerEnterTruckRadius;

    public UnityEvent OnPlayerMove, OnPlayerLook; 

    private void Update() {
        OnPlayerMove?.Invoke();
    }

}
