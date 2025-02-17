using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gasline : MonoBehaviour {

    [Serializable]
    public class GaslinePoints {
        public GameObject location;
        public float speed;
        public float delay;
        public bool canOutput = false;
    }

    public List<GaslinePoints> points;
    public int currentIndex = 0;
    public bool wasLit = false, canMove = false; 
    public GameObject flamePrefab;

    private GameObject flameVisual;
    private bool canSpawnVisual = false; 

    public void Update() {
        if (wasLit) {
            HandleGaslineMovement();
        }
    }

    public void HandleSpawn() {
        if (!canSpawnVisual) {
            flameVisual = Instantiate(flamePrefab, points[currentIndex].location.transform.position, Quaternion.identity);
        }
        canSpawnVisual = true;
    }

    public void HandleGaslineMovement() {
        HandleSpawn();
        if (canMove) {
            float distance = Vector3.Distance(flameVisual.transform.position, points[currentIndex].location.transform.position);
            if (distance < 0.02f) {
                if (currentIndex != points.Count - 1) {
                    StartCoroutine(DelayBeforeNextMove(GetPointDelay(currentIndex)));
                }
            }
            flameVisual.transform.position = Vector3.MoveTowards(flameVisual.transform.position, GetPointPosition(currentIndex), GetPointSpeed(currentIndex));
        }

        if (flameVisual.transform.position == GetPointPosition(currentIndex) && GetPointOutput(currentIndex)) {
            Debug.Log("Pop");
        }
    }

    IEnumerator DelayBeforeNextMove(float delay) {
        canMove = false;
        yield return new WaitForSeconds(delay);
        currentIndex++;
        canMove = true;
    }

    private Vector3 GetPointPosition(int index) { return points[index].location.transform.position; }
    private bool GetPointOutput(int index) { return points[index].canOutput; }
    private float GetPointDelay(int index) { return points[index].delay; }
    private float GetPointSpeed(int index) { return points[index].speed * Time.deltaTime; }
}