using Unity.Cinemachine;
using UnityEngine;

public class CinePriority : MonoBehaviour
{
    CinemachineCamera cam;

    private void Awake()
    {
         cam = GetComponent<CinemachineCamera>();
    }

    public void SetPriority(int priority)
    {
         cam.Priority = priority;
    }    
}
