using Fusion;
using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    public bool isTriggered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Wok bitvh");
        if (other.gameObject.layer == playerLayer && other.gameObject.GetComponent<NetworkObject>().HasInputAuthority)
        {
            
            isTriggered = true;
        }
    }
}
