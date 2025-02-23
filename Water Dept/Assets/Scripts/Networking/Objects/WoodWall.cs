using Fusion;
using System.Collections;
using UnityEngine;

public class WoodWall : NetworkBehaviour {
    public float burnTime = 5f;

    //Network Vars vars are communicated and updated by networkbehaviors
    [Networked] protected bool nIsBurning { get; set; }
    [SerializeField] protected bool isBurning = false;
    private bool isSpawned = false;

    // Lerp burn colors
    private Color woodColor;
    private Color startColor = Color.yellow;
    private Color endColor = Color.red;

    private float timer = 0f;

    public override void Spawned() {
        nIsBurning = isBurning;
        isSpawned = true;
    }

    private void Awake() {
        woodColor = GetComponent<MeshRenderer>().material.color;
    }

    private void Update() {
        if (nIsBurning && isSpawned) {
            timer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(endColor * woodColor, startColor * woodColor, burnTime - timer);
            StartCoroutine(BurnWall(gameObject, burnTime));
        }
    }

    IEnumerator BurnWall(GameObject wall, float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(wall);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Flame")) {
            nIsBurning = true;
        }
    }
}
