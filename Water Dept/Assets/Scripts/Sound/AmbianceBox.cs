using UnityEngine;

public class AmbienceBox : MonoBehaviour
{
    [Tooltip("Area of the sound to be in")]
    public Collider Area;

    void Update()
    {
        Vector3 closestPoint = Area.ClosestPoint(Camera.main.transform.position);
        transform.position = closestPoint;
    }
}
