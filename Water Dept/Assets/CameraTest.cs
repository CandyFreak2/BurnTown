using UnityEngine;

public class CameraTest : MonoBehaviour
{
    [SerializeField] GameObject player;

    private void Start()
    {
        gameObject.transform.position = player.transform.position + new Vector3(31, -60, 0);
    }
    private void Update()
    {
        gameObject.transform.position = player.transform.position + new Vector3(31, -60, 0);
    }
}