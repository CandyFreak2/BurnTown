using System.Collections;
using UnityEngine;

public class ConcreteWall : MonoBehaviour {
    public GameObject originalWall, fractureWall, explosionEffect;
    public float explosionRadius, fragScale;
    public float shrinkTime, modelDestroyTime, explosionTime;

    private GameObject fracturedObject, explosionObject;

    public GameObject FracturedObject { get => fracturedObject; set => fracturedObject = value; }
    public GameObject ExplosionObject { get => explosionObject; set => explosionObject = value; }

     private void Update() {
        // Temporary Triggers until barrel implementation
        if (Input.GetKeyDown(KeyCode.X)) {
            Explode(10);
        }
    }

    public void Explode(float explosiveForce) {
        if (originalWall != null) {
            originalWall.SetActive(false);

            if (fractureWall != null) {
                fracturedObject = Instantiate(fractureWall, fractureWall.transform.position, Quaternion.identity);

                // APPLYING EXPLOSION FORCE TO ALL PIECES OF MODEL AT MODEL'S CENTER POS 
                // WAITING ON BARREL FOR ACTUAL EXPLOSIVE FORCE
                foreach (Transform t in fracturedObject.transform) {
                    if (t.TryGetComponent<Rigidbody>(out var rb)) {
                        rb.AddExplosionForce(explosiveForce, fractureWall.transform.position, explosionRadius);
                    }

                    // THIS SHRINKS THE OBJECTS SO THAT WAY THEY DISSAPEAR AFTER BEING ON THE GROUND
                    StartCoroutine(ShrinkFragments(t, shrinkTime));
                }

                // THE OBJECT IS DESTROYED AFTER MODELDESTROYTIME SECONDS
                Destroy(fracturedObject, modelDestroyTime);

                if (explosionEffect != null) {
                    explosionObject = Instantiate(explosionEffect, originalWall.transform.position, Quaternion.identity);
                    Destroy(explosionObject, explosionTime);
                }
            }
        }
    }

    public void Reset() {
        Destroy(fracturedObject);
        Destroy(explosionObject);
        originalWall.SetActive(true);
        originalWall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    IEnumerator ShrinkFragments(Transform t, float delay) {
        yield return new WaitForSeconds(delay);
        if (t != null) {
            Vector3 modelScale = t.localScale;
            while (modelScale.x >= 0) {
                modelScale -= FloatToVec3(fragScale);
                if (t != null) {
                    t.localScale = modelScale;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private Vector3 FloatToVec3(float value) {
        return new Vector3(value, value, value);
    }
}
