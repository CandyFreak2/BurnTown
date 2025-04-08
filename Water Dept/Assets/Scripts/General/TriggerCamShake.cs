using UnityEngine;

public class TriggerCamShake : MonoBehaviour
{
    public float strangth;
    public float duration;

    public void TriggerShake()
    {
        CinemachineShake.ShakeAllCameras(strangth, duration);   
    }
}
