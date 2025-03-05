using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class AudioPlayer : MonoBehaviour
{
    static AudioSource StaticSFXObject;
    [SerializeField] AudioSource SFXObject;

    private void Awake()
    {
        if (StaticSFXObject == null && SFXObject != null) 
            StaticSFXObject = SFXObject;
    }

    #region SingleSoundWorldPostion
    /// <summary>
    /// play a sound effect
    /// </summary>
    public static AudioSource PlaySFX(AudioClip clip, Vector3 spawnPos, float pitchWobble = 0.0f, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(StaticSFXObject, spawnPos, Quaternion.identity);

        audioSource.clip = clip;
        audioSource.volume = volume;

        if (Mathf.Abs(pitchWobble) > 0)
            audioSource.pitch = audioSource.pitch + Random.Range(-pitchWobble * 0.5f, pitchWobble * 0.5f);

        audioSource.Play();
        DontDestroyOnLoad(audioSource.gameObject);
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }

    /// <summary>
    /// play SFX at random from an array
    /// </summary>
    public static void PlaySFX(AudioClip[] clips, Vector3 spawnPos, float pitchWobble = 0.0f, float volume = 1f)
    {
        if (clips.Length == 0)
            return;

        //select clip to play from a random range. 
        PlaySFX(clips[Random.Range(0, clips.Length)], spawnPos, pitchWobble, volume);
    }

    /// <summary>
    /// play SFX at random from a list
    /// </summary>
    public static void PlaySFX(List<AudioClip> clips, Vector3 spawnPos, float pitchWobble = 0.0f, float volume = 1f)
    {
        if (clips.Count == 0)
            return;

        //select clip to play from a random range. 
        PlaySFX(clips[Random.Range(0, clips.Count)], spawnPos, pitchWobble, volume);
    }
    #endregion
    #region SingleSoundTransform
    /// <summary>
    /// play a sound effect and attach it to a parent
    /// </summary>
    public static AudioSource PlaySFX(AudioClip clip, Transform attachPoint, float pitchWobble = 0.0f, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(StaticSFXObject, attachPoint);

        audioSource.clip = clip;
        audioSource.volume = volume;

        if (Mathf.Abs(pitchWobble) > 0)
            audioSource.pitch = audioSource.pitch + Random.Range(-pitchWobble, pitchWobble);

        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }

    /// <summary>
    /// play SFX at random from an array and attatch it to a parent
    /// </summary>
    public static void PlaySFX(AudioClip[] clips, Transform attachPoint, float pitchWobble = 0.0f, float volume = 1f)
    {
        if (clips.Length == 0)
            return;

        //select clip to play from a random range. 
        PlaySFX(clips[Random.Range(0, clips.Length)], attachPoint, pitchWobble, volume);
    }

    /// <summary>
    /// play SFX at random from a list and attatch it to a parent
    /// </summary>
    public static void PlaySFX(List<AudioClip> clips, Transform attachPoint, float pitchWobble = 0.0f, float volume = 1f)
    {
        if (clips.Count == 0)
            return;

        //select clip to play from a random range. 
        PlaySFX(clips[Random.Range(0, clips.Count)], attachPoint, pitchWobble, volume);
    }
    #endregion
    #region LoopingSound
    /// <summary>
    /// play a looping SFX
    /// </summary>
    public static AudioSource PlaySFXLoop(AudioClip clip, Vector3 spawnPos, float startVolume = 0f)
    {
        AudioSource audioSource = Instantiate(StaticSFXObject, spawnPos, Quaternion.identity);

        audioSource.clip = clip;
        audioSource.volume = startVolume;
        audioSource.loop = true;

        audioSource.Play();
        DontDestroyOnLoad(audioSource.gameObject);
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }
    /// <summary>
    /// play a random looping SFX from an array
    /// </summary>
    public static AudioSource PlaySFXLoop(AudioClip[] clips, Vector3 spawnPos, float startVolume = 0f)
    {
        if (clips.Length == 0)
            return null;

        //select clip to play from a random range. 
        return PlaySFXLoop(clips[Random.Range(0, clips.Length)], spawnPos, startVolume);
    }

    /// <summary>
    /// play a random looping SFX from a list
    /// </summary>
    public static AudioSource PlaySFXLoop(List<AudioClip> clips, Vector3 spawnPos, float startVolume = 0f)
    {
        if (clips.Count == 0)
            return null;

        //select clip to play from a random range. 
        return PlaySFXLoop(clips[Random.Range(0, clips.Count)], spawnPos, startVolume);
    }

    /// <summary>
    /// play a looping SFX and attatch it to an object
    /// </summary>
    public static AudioSource PlaySFXLoop(AudioClip clip, Transform attachObj, float startVolume = 0f)
    {
        AudioSource audioSource = Instantiate(StaticSFXObject, attachObj);

        audioSource.clip = clip;
        audioSource.volume = startVolume;
        audioSource.loop = true;

        audioSource.Play();
        DontDestroyOnLoad(audioSource.gameObject);
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }
    /// <summary>
    /// play a random looping SFX from an array
    /// </summary>
    public static AudioSource PlaySFXLoop(AudioClip[] clips, Transform attachObj, float startVolume = 0f)
    {
        if (clips.Length == 0)
            return null;

        //select clip to play from a random range. 
        return PlaySFXLoop(clips[Random.Range(0, clips.Length)], attachObj, startVolume);
    }

    /// <summary>
    /// play a random looping SFX from a list and attatch to transfom
    /// </summary>
    public static AudioSource PlaySFXLoop(List<AudioClip> clips, Transform attachObj, float startVolume = 0f)
    {
        if (clips.Count == 0)
            return null;

        //select clip to play from a random range. 
        return PlaySFXLoop(clips[Random.Range(0, clips.Count)], attachObj, startVolume);
    }

    /// <summary>
    /// fade out a sound source from full volume
    /// </summary>
    public static IEnumerator FadeAudioOut(AudioSource source, float duration = 0.5f, bool destroyComponentOnFinish = false)
    {
        WaitForFixedUpdate tick = new();

        for(float i = 0; source.volume > 0; i += Time.fixedDeltaTime)
        {
            yield return tick;
            source.volume = Mathf.Lerp(duration, 0, i);
        }

        if (destroyComponentOnFinish)
            Destroy(source);
    }
    /// <summary>
    /// fade in a sound source from zero volume
    /// </summary>
    public static IEnumerator FadeAudioIn(AudioSource source, float duration = 0.5f)
    {
        WaitForFixedUpdate tick = new();

        for (float i = 0; source.volume < 1; i += Time.fixedDeltaTime)
        {
            yield return tick;
            source.volume = Mathf.Lerp(0, duration, i);
        }
    }
    #endregion
}
