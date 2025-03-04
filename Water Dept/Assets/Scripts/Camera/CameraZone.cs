using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    //[SerializeField] string triggerTag = "Player";
    CinemachineCamera cam;

    private void Awake()
    {
        cam = GetComponentInChildren<CinemachineCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) 
            return;

        if(player.HasInputAuthority)
        {
            cam.Priority = 10;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null)
            return;

        if (player.HasInputAuthority)
        {
            cam.Priority = 1;
        }
    }
}
