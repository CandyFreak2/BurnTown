using UnityEngine;

public class TriggerCamShake : MonoBehaviour
{
    public float strangth;
    public float duration;
    public CinemachineShake cam;

    public void TriggerShake()
    {
        cam.ShakeCamera(strangth, duration);   
    }
}
