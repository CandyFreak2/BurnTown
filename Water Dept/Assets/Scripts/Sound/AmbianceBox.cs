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

        //DO NO SCALE AUDIO ZONES :^) 
        //Use the box collider volume tool pls
        Area.transform.localScale = Vector3.one;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        Vector3 closestPoint = Area.ClosestPoint(Camera.main.transform.position);
        closestPoint += Camera.main.transform.forward * offset;
        transform.position = closestPoint;
    }
}
