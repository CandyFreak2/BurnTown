using UnityEngine;
using System.Collections.Generic;

public class TriggerSoundClip : MonoBehaviour
{
    [SerializeField, Range(0, 1)] public float volume = 1f;
    [SerializeField, Range(0, .5f)] public float wobble = 0f;
    public Transform target;
    public List<AudioClip> clips;

    private void Awake()
    {
        if(target == null)
            target = transform;
    }

    public void PlayRandomClip()
    {
        AudioPlayer.PlaySFX(clips, target.transform, wobble, volume);
    }
}
