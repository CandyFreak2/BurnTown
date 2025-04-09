using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    [Header("Virtual Cameras")]
    [SerializeField] public CinemachineCamera startCam;
    [SerializeField] public CinemachineCamera mainCam;
    [SerializeField] public CinemachineCamera lobbyCam;

    [Header("References")]
    [SerializeField] GameObject startTXT;
    [SerializeField] GameObject mainMenu;

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
                Invoke("EnableMenu", 0.2f);
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

    void EnableMenu()
    {
        mainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainMenu.transform.GetChild(0).gameObject);
    }
}
