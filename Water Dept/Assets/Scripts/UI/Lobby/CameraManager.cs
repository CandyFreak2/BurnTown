using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [Header("Virtual Cameras")]
    [SerializeField] public CinemachineCamera startCam;
    [SerializeField] public CinemachineCamera mainCam;
    [SerializeField] public CinemachineCamera lobbyCam;

    [Header("References")]
    [SerializeField] GameObject startTXT;
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] Canvas mainMenuCanvas;
    [SerializeField] GameObject playButton;

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

        //set main canvas to disable
        mainMenuCanvas.enabled = false;
    }
    private void Update()
    {
        if(currentCam == startCam)
        {
            //play button is not interactable
            DisableUI();

            if(Input.anyKeyDown) //if the player presses anything it will transition to main screen camera
            {
                SwitchActiveCamera(mainCam);
                startTXT.SetActive(false);
                buttons.ForEach(n => n.SetActive(true));
            }
        }
        if(currentCam == mainCam)
        {
            mainMenuCanvas.enabled = true;

            Invoke("EnableUI", 1f);
        }
        else
        {
            mainMenuCanvas.enabled = false;
            DisableUI();
            
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

    void EnableUI()
    {
        //play button is interactable
        playButton.GetComponent<Button>().interactable = true;
    }

    void DisableUI()
    {
        //play button is not interactable
        playButton.GetComponent<Button>().interactable = false;
    }

}
