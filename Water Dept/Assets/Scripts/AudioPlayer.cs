using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    static AudioSource StaticSFXObject;
    [SerializeField] AudioSource SFXObject;

    private void Awake()
    {
        if (StaticSFXObject == null && SFXObject != null) 
            StaticSFXObject = SFXObject;
    }

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
    /// play a sound effect and attatch it to a parent
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
    public static void PlaySFX(AudioClip[] clips, Transform attachPoint,float pitchWobble = 0.0f, float volume = 1f)
    {
        if (clips.Length == 0)
            return;

        //select clip to play from a random range. 
        PlaySFX(clips[Random.Range(0, clips.Length)], attachPoint, pitchWobble, volume);
    }
}
