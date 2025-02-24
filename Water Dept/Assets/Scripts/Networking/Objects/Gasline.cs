using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Gasline : NetworkBehaviour {

    [Serializable]
    public class GaslinePoints {
        [Tooltip("Location of Point")] public GameObject location;
        [Tooltip("Movement Speed of Flame Visual")] public float speed;
        [Tooltip("Once visual reaches point, will delay in seconds")] public float delay;
        [Tooltip("T - Allows gas to pass through\nF - Blocks Gas")] public bool canPop = false;
        [Tooltip("Enable for the starting index")] public bool canStart = false;
        [Tooltip("Ignore if canPop is false")] public Vector3 popDirection = new(0, 90, 0);
    }

    [Header("Gasline Points")]
    public List<GaslinePoints> points;

    [Header("Controls")]
    [Networked] public bool wasLit { get; set; }
    [Networked] public bool canMove { get; set; }
    [Networked] public int currentIndex { get; set; }
    [Networked] public int startIndex { get; set; }
    [SerializeField] public Burnable endGoal;

    [Header("Prefabs")]
    public GameObject flamePrefab;
    public GameObject firePrefab;

    private NetworkObject flameVisual, fireVisual;
    [Networked] private NetworkBool canSpawnFlame { get; set; }
    [Networked] private NetworkBool wasSpawned { get; set; }

    // Keeps track of which points have had fireVisual spawned
    private bool[] fireSpawned;

    // Tolerance is used incase points and flameVisual don't 100% line up and counteracts floating point precision 
    private const float tolerance = 0.1f;

    // Grabs all gasline points
    private GaslinePoint[] gaslinePoints;

    // Network
    private bool isSpawned = false;

    // Initalizes fireSpawned array with dynamic amount, ie the length of the points list
    public override void Spawned() {
        isSpawned = true;
        fireSpawned = new bool[points.Count];
        gaslinePoints = GetComponentsInChildren<GaslinePoint>();
    }

    public override void FixedUpdateNetwork() {
        if (isSpawned && Object.HasStateAuthority) {
            if (!wasSpawned) {
                StartFireSpread(startIndex);
            }

            if (wasLit) {
                HandleGaslineMovement();
            }
        }
    }

    public void SpawnFlameVisual() {
        if (flameVisual == null && !canSpawnFlame) {
            flameVisual = Runner.Spawn(flamePrefab, points[currentIndex].location.transform.position, Quaternion.identity);
            canSpawnFlame = true;
        }
    }

    // Function that handles gasline movement
    public void HandleGaslineMovement() {
        SpawnFlameVisual();

        if (flameVisual != null) {
            if (canMove) {
                float distance = Vector3.Distance(flameVisual.transform.position, points[currentIndex].location.transform.position);
                if (distance < 0.02f) {
                    // If the currentIndex has not reached the end, allow delays to occur when at points
                    if (currentIndex != points.Count - 1) {
                        StartCoroutine(DelayBeforeNextMove(GetPointDelay(currentIndex)));
                    }
                }
                // Moves the flameVisual based on Vec3 Math and indexes of the points list
                Vector3 direction = Vector3.MoveTowards(flameVisual.transform.position, GetPointPosition(currentIndex), GetPointSpeed(currentIndex));
                flameVisual.transform.forward = (direction - flameVisual.transform.position).normalized;
                flameVisual.transform.position = direction;
            }
            HandlePointFire2();
        }
    }

    public void HandlePointFire2() {
        // Flame Visual - If flame is on gasline point, the valve can pop and within general radius of the location of points
        if (flameVisual.GetComponent<GaslineSpark>().isOnGaslinePoint && CanIndexPop(currentIndex) && (Vector3.Distance(flameVisual.transform.position, GetPointPosition(currentIndex)) < tolerance)) {
            Vector3 popDirection = GetPopDirection(currentIndex);
            Vector3 spawnPosition = GetPointPosition(currentIndex);

            // Calculates the direction of the fire visual prefab and applies it to the rotation
            if (popDirection != Vector3.zero && !fireSpawned[currentIndex]) {
                if (!fireSpawned[currentIndex]) {
                    fireVisual = Runner.Spawn(firePrefab, spawnPosition, Quaternion.LookRotation(popDirection));
                    fireSpawned[currentIndex] = true;
                }

                if (fireVisual != null) {
                    fireVisual.transform.rotation = Quaternion.LookRotation(popDirection);
                }

                // Plays VFX prefab
                ParticleSystem fireVisualVFX = fireVisual.GetComponentInChildren<ParticleSystem>();
                fireVisualVFX.Play();

                // Stops VFX after duration is reached
                StartCoroutine(TurnOffFireVFX(fireVisualVFX.main.duration, fireVisualVFX));
            }
        }
    }

    public void HandlePointFire() {
        if (flameVisual.GetComponent<GaslineSpark>().isOnGaslinePoint) {
            if (CanIndexPop(currentIndex) && Vector3.Distance(flameVisual.transform.position, GetPointPosition(currentIndex)) < tolerance) {
                Vector3 popDirection = GetPopDirection(currentIndex);
                Vector3 spawnPosition = GetPointPosition(currentIndex);

                if (popDirection != Vector3.zero && !fireSpawned[currentIndex]) {
                    fireSpawned[currentIndex] = true;
                    RpcHandleFireSpawn(spawnPosition, popDirection, currentIndex);
                }
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcHandleFireSpawn(Vector3 position, Vector3 direction, int index) {
        if (!fireSpawned[index]) {
            fireVisual = Runner.Spawn(firePrefab, position, Quaternion.LookRotation(direction));
            fireSpawned[index] = true;
        }

        if (fireVisual != null) {
            fireVisual.transform.rotation = Quaternion.LookRotation(direction);

            // Plays VFX prefab
            ParticleSystem fireVisualVFX = fireVisual.GetComponent<ParticleSystem>();
            fireVisualVFX.Play();

            // Stops VFX after duration is reached
            StartCoroutine(TurnOffFireVFX(fireVisualVFX.main.duration, fireVisualVFX));

            // Handles enabling burning
            if (currentIndex == 2) {
                endGoal.nIsBurning = true;
                Debug.Log(endGoal.nIsBurning);
            }
        }
    }

    public void StartFireSpread(int index) {
        if (gaslinePoints[index].GetIsOnFire() && points[index].canStart) {
            wasLit = true;
            canMove = true;
            wasSpawned = true;
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
        Runner.Despawn(vfx.transform.parent.GetComponent<NetworkObject>());
       
    }

    // Helper Functions
    private Vector3 GetPointPosition(int index) { return points[index].location.transform.position; }
    private Vector3 GetPopDirection(int index) { return points[index].popDirection; }
    private bool CanIndexPop(int index) { return points[index].canPop; }
    private float GetPointDelay(int index) { return points[index].delay; }
    private float GetPointSpeed(int index) { return points[index].speed * Time.deltaTime; }
    private int GetPointLength() { return points.Count - 1; }
}
