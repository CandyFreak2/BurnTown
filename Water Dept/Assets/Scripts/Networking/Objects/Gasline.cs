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

    [Header("Gasline Points")]
    public List<GaslinePoints> points;

    [Header("Controls")]
    public int currentIndex = 0;
    public bool wasLit = false, canMove = false; 

    [Header("Prefabs")]
    public GameObject flamePrefab;
    public GameObject firePrefab;

    private GameObject flameVisual, fireVisual;
    private bool canSpawnFlame = false;

    // Keeps track of which points have had fireVisual spawned
    private bool[] fireSpawned;

    // Tolerance is used incase points and flameVisual don't 100% line up and counteracts floating point precision 
    private const float tolerance = 0.1f;

    // Initalizes fireSpawned array with dynamic amount, ie the length of the points list
    public void Start() {
        fireSpawned = new bool[points.Count];
    }

    public void Update() {
        if (wasLit) {
            HandleGaslineMovement();
        }
    }

    // Helper function for initalizating prefabs but allows reference to objects for usage
    public void HandleSpawn(ref bool canSpawn, ref GameObject visual, GameObject prefab, Vector3 pos, Quaternion rot) {
        if (!canSpawn) {
            visual = Instantiate(prefab, pos, rot);
        }
        canSpawn = true;
    }

    // Function that handles gasline movement
    public void HandleGaslineMovement() {
        HandleSpawn(ref canSpawnFlame, ref flameVisual, flamePrefab, points[currentIndex].location.transform.position, Quaternion.identity);
        flameVisual.GetComponentInChildren<ParticleSystem>().Play();
        if (canMove) {
            float distance = Vector3.Distance(flameVisual.transform.position, points[currentIndex].location.transform.position);
            if (distance < 0.02f) {
                // If the currentIndex has not reached the end, allow delays to occur when at points
                if (currentIndex != points.Count - 1) {
                    StartCoroutine(DelayBeforeNextMove(GetPointDelay(currentIndex)));
                }
            }
            // Moves the flameVisual based on Vec3 Math and indexes of the points list
            flameVisual.transform.position = Vector3.MoveTowards(flameVisual.transform.position, GetPointPosition(currentIndex), GetPointSpeed(currentIndex));
        }

        // Flame Visual - If flame is on gasline point, the valve can pop and within general radius of the location of points
        if (flameVisual.GetComponent<GaslineSpark>().GetIsOnGaslinePoint() && CanIndexPop(currentIndex) && Vector3.Distance(flameVisual.transform.position, GetPointPosition(currentIndex)) < tolerance) {
            Vector3 popDirection = GetPopDirection(currentIndex);
            Vector3 spawnPosition = GetPointPosition(currentIndex);

            // Calculates the direction of the fire visual prefab and applies it to the rotation
            if (popDirection != Vector3.zero && !fireSpawned[currentIndex]) {
                HandleSpawn(ref fireSpawned[currentIndex], ref fireVisual, firePrefab, spawnPosition, Quaternion.LookRotation(popDirection));

                if (fireVisual != null) {
                    fireVisual.transform.rotation = Quaternion.LookRotation(popDirection);
                }

                // Plays VFX prefab
                ParticleSystem fireVisualVFX = fireVisual.GetComponent<ParticleSystem>();
                fireVisualVFX.Play();

                // Stops VFX after duration is reached
                StartCoroutine(TurnOffFireVFX(fireVisualVFX.main.duration, fireVisualVFX));
            }
        }
    }

    // Stops FlameVisual movement, starts delay, increments currentIndex and re-enables movement
    IEnumerator DelayBeforeNextMove(float delay) {
        canMove = false;
        yield return new WaitForSeconds(delay);
        currentIndex++;
        canMove = true;
    }

    // Turns off fireVFX
    IEnumerator TurnOffFireVFX(float delay, ParticleSystem vfx) {
        yield return new WaitForSeconds(delay);
        vfx.Stop();
    }

    // Helper Functions
    private Vector3 GetPointPosition(int index) { return points[index].location.transform.position; }
    private Vector3 GetPopDirection(int index) { return points[index].popDirection; }
    private bool CanIndexPop(int index) { return points[index].canPop; }
    private float GetPointDelay(int index) { return points[index].delay; }
    private float GetPointSpeed(int index) { return points[index].speed * Time.deltaTime; }
}