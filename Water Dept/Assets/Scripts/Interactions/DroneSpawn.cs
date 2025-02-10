using UnityEngine;

public class DroneSpawn : MonoBehaviour {
    public int numOfDrones = 10;
    public float minRadius = 15f, maxRadius = 25f;
    public float minHeight = 0.5f, spawnHeight = 1.0f;
    public GameObject dronePrefab;

    private GameObject[] players;

    // Grabs all players and spawns drones around the average position 
    public void Awake() {
        players = GameObject.FindGameObjectsWithTag("Player");

        Vector3 averagePos = GetAveragePlayerPos();

        for (int i = 0; i < numOfDrones; i++) {
            Vector3 spawnPos = GetSpawnablePos(averagePos);
            Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        }
    }

    // Calculates the average player position
    private Vector3 GetAveragePlayerPos() {
        Vector3 sum = Vector3.zero;
        foreach (GameObject player in players) {
            sum += player.transform.position;
        }
        return sum / players.Length;
    }

    // Loops around a radius and checks if the position is viable
    private Vector3 GetSpawnablePos(Vector3 center) {
        Vector3 spawnPos;
        bool isValid;

        do {
            Vector2 radius = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
            spawnPos = new Vector3(center.x + radius.x, spawnHeight, center.z + radius.y) {
                y = Mathf.Max(spawnHeight, minHeight)
            };

            isValid = Vector3.Distance(spawnPos, center) >= minRadius;
        } while (!isValid);

        return spawnPos;
    }
}
