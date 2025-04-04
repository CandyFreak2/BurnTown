using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin _ChannelPerlin;
    private CamShake activeShake;
    private Coroutine shakeCorutine;

    private void Awake()
    {
        _ChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        activeShake = new CamShake(0, 0, _ChannelPerlin);
    }

    public static void ShakeAllCameras(float intensity, float duration)
    {
        CineSingleCamera.GetActiveCamera().GetComponent<CinemachineShake>().ShakeCamera(intensity, duration);

        /*foreach(CineSingleCamera cam in CineSingleCamera.activeCineCameras)
        {
            CinemachineShake shake = cam.GetComponentInParent<CinemachineShake>();
            if(shake != null)
                shake.ShakeCamera(intensity, duration);
        }*/
    }

    public void ShakeCamera(float intensity, float duration)
    {
        //filter out lower intensity shake
        if (intensity < activeShake.target.AmplitudeGain) 
            return;

        if(shakeCorutine != null)
            StopCoroutine(shakeCorutine);
        
        activeShake = new CamShake(intensity, duration, _ChannelPerlin);
        shakeCorutine = StartCoroutine(activeShake.RunShake());
    }

    private class CamShake 
    {
        public float intensity;
        public float duration;
        public CinemachineBasicMultiChannelPerlin target;

        public CamShake(float intensity, float duration, CinemachineBasicMultiChannelPerlin target)
        {
            this.intensity = intensity;
            this.duration = duration;
            this.target = target;   
        }

        public IEnumerator RunShake()
        {
            WaitForSeconds tick = new WaitForSeconds(0.02f);
            target.AmplitudeGain = intensity;

            //fade out shake over time
            for(float t = duration; target.AmplitudeGain > 0; t -= 0.02f)
            {
                yield return tick;
                target.AmplitudeGain = Mathf.Lerp(0, intensity, Mathf.InverseLerp(0, duration, t));
            }
        }
    }
}
