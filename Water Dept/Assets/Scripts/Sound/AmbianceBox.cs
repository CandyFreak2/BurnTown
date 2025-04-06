using UnityEngine;

public class AmbienceBox : MonoBehaviour
{
    [Tooltip("Area of the sound to be in")]
    public BoxCollider Area;

    [Tooltip("How far to offset the sfx infront of the camera")]
    [SerializeField, Range(0f, 20f)] float offset = 5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Area.transform.position + Area.center, Area.size);
    }

    void Update()
    {
        Vector3 closestPoint = Area.ClosestPoint(Camera.main.transform.position);
        closestPoint += Camera.main.transform.forward * offset;
        transform.position = closestPoint;
    }
}
