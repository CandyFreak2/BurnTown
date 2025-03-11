using UnityEngine;
using System.Collections.Generic;

public class TriggerSoundClip : MonoBehaviour
{
    [SerializeField, Range(0, 1)] public float volume = 1f;
    [SerializeField, Range(0, .5f)] public float wobble = 0f;
    public List<AudioClip> clips;

    public void PlayRandomClip()
    {
        AudioPlayer.PlaySFX(clips, transform, wobble, volume);
    }
}
