using System;
using System.Collections.Generic;
using UnityEngine;

public class Gasline : MonoBehaviour {

    [Serializable]
    public class GaslinePoints {
        public GameObject location;
        public float speed;
        public bool canOutput = false;
    }

    public List<GaslinePoints> points;
    public int currentIndex = 0;
    public int endPoint;
    public bool wasLit = false, canMove = false; 
    public GameObject flamePrefab;

    private GameObject flameVisual;
    private bool canSpawnVisual = false; 

    public void Update() {
        if (wasLit) {
            HandleGaslineMovement();
        }
    }

    public void HandleGaslineMovement() {
        HandleSpawn();

        if (canMove) {
            if (HasReachedEndPoint()) {
                MoveToNextPoint();
            }
            MoveTowardsEndPoint();
        }
    }

    public void HandleSpawn() {
        if (!canSpawnVisual) {
            flameVisual = Instantiate(flamePrefab, points[currentIndex].location.transform.position, Quaternion.identity);
        }
        canSpawnVisual = true;
    }

    private bool HasReachedEndPoint() {
        float distance = Vector3.Distance(flameVisual.transform.position, points[currentIndex].location.transform.position);
        return distance < 0.02f;
    }

    private void MoveToNextPoint() {
        currentIndex++;
        if (currentIndex >= points.Count) {
            currentIndex = 0;
        }
    }

    void MoveTowardsEndPoint() {
        flameVisual.transform.position = Vector3.MoveTowards(flameVisual.transform.position, GetPointPosition(currentIndex), GetPointSpeed(currentIndex) * Time.deltaTime);
    }

    private Vector3 GetPointPosition(int index) {
        return points[index].location.transform.position;
    }

    private float GetPointSpeed(int index) {
        return points[index].speed;
    }
}