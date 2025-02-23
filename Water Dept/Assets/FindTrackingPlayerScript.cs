using UnityEngine;
using Unity.Cinemachine;

public class FindTrackingPlayerScript : MonoBehaviour
{
    public GameObject player;
    CinemachineCamera playerCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        playerCam = GetComponent<CinemachineCamera>();
        
    }

    // Update is called once per frame
    void Update()
    {
        playerCam.Target.TrackingTarget = player.transform;
    }
}