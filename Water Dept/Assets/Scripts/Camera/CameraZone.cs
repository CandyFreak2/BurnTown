using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    //[SerializeField] string triggerTag = "Player";
    CinemachineCamera cam;
    [SerializeField] GameObject mainPlayer;


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
            mainPlayer = player.gameObject;
            cam.Priority = 10;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != mainPlayer)
            return;

        cam.Priority = -1;
    }
}
