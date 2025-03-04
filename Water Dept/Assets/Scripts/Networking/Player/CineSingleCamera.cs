using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CineSingleCamera : MonoBehaviour
{
    public static List<CineSingleCamera> activeCineCameras = new List<CineSingleCamera>();

    private CinemachineCamera _PlayerCam;

    private void Awake()
    {
        _PlayerCam = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        activeCineCameras.Add(this);
    }

    private void OnDisable()
    {
        activeCineCameras.Remove(this);
    }

    public void SetAsActiveCamera()
    {
        foreach (var cam in activeCineCameras)
            cam._PlayerCam.enabled = false;

        _PlayerCam.enabled = true;
    }
}
