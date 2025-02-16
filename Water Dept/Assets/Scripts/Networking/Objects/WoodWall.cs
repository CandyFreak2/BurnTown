using System.Collections;
using UnityEngine;

public class WoodWall : MonoBehaviour {
    public float burnTime = 5f;
    private bool isBurning = false;

    // Lerp burn colors
    private Color woodColor;
    private Color startColor = Color.yellow;
    private Color endColor = Color.red;

    private float timer = 0f;

    private void Awake() {
        woodColor = GetComponent<MeshRenderer>().material.color;
    }

    private void Update() {
        if (isBurning) {
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
            isBurning = true;
        }
    }
}
