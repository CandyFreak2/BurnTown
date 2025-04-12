using UnityEngine;

public class RotateOnAwake : MonoBehaviour
{
    public Vector3 axis = Vector3.up;

    private void Start()
    {
        transform.Rotate(axis, Random.Range(0, 360));    
    }
}
