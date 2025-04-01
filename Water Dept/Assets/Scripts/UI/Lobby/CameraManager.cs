using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Virtual Cameras")]
    [SerializeField] public CinemachineCamera startCam;
    [SerializeField] public CinemachineCamera mainCam;
    [SerializeField] public CinemachineCamera lobbyCam;

    [Header("References")]
    [SerializeField] GameObject startTXT;

    private CinemachineCamera currentCam;

    void Start()
    {
        //turn on cameras
        startCam.enabled = true;
        lobbyCam.enabled = true;
        mainCam.enabled = true;

        // Set priorities
        startCam.Priority = 10;
        mainCam.Priority = 0;
        lobbyCam.Priority = 0;

        currentCam = startCam;
    }
    private void Update()
    {
        if(currentCam == startCam)
        {
            if(Input.anyKeyDown) //if the player presses anything it will transition to main screen camera
            {
                SwitchActiveCamera(mainCam);
                startTXT.SetActive(false);
            }
        }
    }

    public void SwitchActiveCamera(CinemachineCamera newVCam)
    {
        // Reset priorities
        startCam.Priority = 0;
        mainCam.Priority = 0;
        lobbyCam.Priority = 0;

        // Set new camera as highest priority
        newVCam.Priority = 10;
        currentCam = newVCam;
    }


}
