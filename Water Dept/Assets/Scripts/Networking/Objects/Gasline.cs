using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gasline : MonoBehaviour {

    [Serializable]
    public class GaslinePoints {
        [Tooltip("Location of Point")] public GameObject location;
        [Tooltip("Movement Speed of Flame Visual")] public float speed;
        [Tooltip("Once visual reaches point, will delay in seconds")] public float delay;
        [Tooltip("T - Allows gas to pass through\nF - Blocks Gas")] public bool canPop = false;
        [Tooltip("Ignore if canPop is false")] public Vector3 popDirection = new(0, 90, 0);
    }

    public List<GaslinePoints> points;
    public int currentIndex = 0;
    public bool wasLit = false, canMove = false; 
    public GameObject flamePrefab;
    public GameObject firePrefab;

    private GameObject flameVisual, fireVisual;
    private bool canSpawnFlame = false;

    // Keeps track of which points have had fireVisual spawned
    private bool[] fireSpawned;

    // Tolerance is used incase points and flameVisual don't 100% line up and counteracts floating point precision 
    private const float tolerance = 0.1f;

    public void Start() {
        fireSpawned = new bool[points.Count];
    }

    public void Update() {
        if (wasLit) {
            HandleGaslineMovement();
        }
    }

    public void HandleSpawn(ref bool canSpawn, ref GameObject visual, GameObject prefab, Vector3 pos, Quaternion rot) {
        if (!canSpawn) {
            visual = Instantiate(prefab, pos, rot);
        }
        canSpawn = true;
    }

    public void HandleGaslineMovement() {
        HandleSpawn(ref canSpawnFlame, ref flameVisual, flamePrefab, points[currentIndex].location.transform.position, Quaternion.identity);
        if (canMove) {
            float distance = Vector3.Distance(flameVisual.transform.position, points[currentIndex].location.transform.position);
            if (distance < 0.02f) {
                if (currentIndex != points.Count - 1) {
                    StartCoroutine(DelayBeforeNextMove(GetPointDelay(currentIndex)));
                }
            }
            flameVisual.transform.position = Vector3.MoveTowards(flameVisual.transform.position, GetPointPosition(currentIndex), GetPointSpeed(currentIndex));
        }

        if (flameVisual.GetComponent<GaslineSpark>().GetIsOnGaslinePoint() && CanIndexPop(currentIndex) && Vector3.Distance(flameVisual.transform.position, GetPointPosition(currentIndex)) < tolerance) {
            Vector3 popDirection = GetPopDirection(currentIndex);
            Vector3 spawnPosition = GetPointPosition(currentIndex);

            if (popDirection != Vector3.zero && !fireSpawned[currentIndex]) {
                HandleSpawn(ref fireSpawned[currentIndex], ref fireVisual, firePrefab, spawnPosition, Quaternion.LookRotation(popDirection));

                if (fireVisual != null) {
                    fireVisual.transform.rotation = Quaternion.LookRotation(popDirection);
                }

                fireVisual.GetComponent<ParticleSystem>().Play();
            }
        }
    }

    IEnumerator DelayBeforeNextMove(float delay) {
        canMove = false;
        yield return new WaitForSeconds(delay);
        currentIndex++;
        canMove = true;
    }

    private Vector3 GetPointPosition(int index) { return points[index].location.transform.position; }
    private Vector3 GetPopDirection(int index) { return points[index].popDirection; }
    private bool CanIndexPop(int index) { return points[index].canPop; }
    private float GetPointDelay(int index) { return points[index].delay; }
    private float GetPointSpeed(int index) { return points[index].speed * Time.deltaTime; }
}