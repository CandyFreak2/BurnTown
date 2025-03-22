using Fusion;
using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private int playerLayerValue;
    public bool isTriggered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isTriggered = false;
        playerLayerValue = (int)Mathf.Log(playerLayer.value, 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerValue)
        {
            NetworkObject inputAuth = other.transform.parent.GetComponent<NetworkObject>();

            if (inputAuth != null && inputAuth.HasInputAuthority)
            {
                isTriggered = true;
            }
        }
    }
}
