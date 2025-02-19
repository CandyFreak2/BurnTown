using UnityEngine;

public class ConcreteWallManager : MonoBehaviour {
    public GameObject fractureEvents;

    private ConcreteWall[] cachedObjects;

    private void Start() {
        cachedObjects = fractureEvents.GetComponents<ConcreteWall>();
    }

    private void Update() {
        foreach (ConcreteWall wall in cachedObjects) {
            BoxCollider[] fractures = wall.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider box in fractures) {
                
            }
        }
    }
}
