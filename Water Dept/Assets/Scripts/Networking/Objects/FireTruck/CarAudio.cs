using UnityEngine;
using System.Collections.Generic;
using System;

public class CarAudio : MonoBehaviour
{
    [SerializeField] Rigidbody car;
    [SerializeField] int currentGear = 0;
    [SerializeField] List<CarGears> gears;

    [Serializable] class CarGears
    {
        public string name = "N";
        public float speedCap;
        public AudioClip sound;
        [HideInInspector] public AudioSource source;
    }

    private void Awake()
    {
        foreach (CarGears gear in gears)
            gear.source = AudioPlayer.PlaySFXLoop(gear.sound, transform, 0);
    }

    private void FixedUpdate()
    {
        SetGear();
    }

    void SetGear()
    {
        float gearStartSpeed = currentGear >= 1 ? gears[currentGear - 1].speedCap : 0;
        float gearMaxSpeed = gears[currentGear].speedCap;
        float gearRatio = Mathf.InverseLerp(gearStartSpeed, gearMaxSpeed, car.linearVelocity.magnitude);

        gears[currentGear].source.pitch = Mathf.Lerp(1f, 2f, gearRatio);

        //check for gear change
        int foundGear = FindGear();
        if(foundGear == currentGear)
            return;

        StartCoroutine(AudioPlayer.FadeAudioOut(gears[currentGear].source));
        StartCoroutine(AudioPlayer.FadeAudioIn(gears[foundGear].source));

        currentGear = foundGear;
    }

    int FindGear()
    {
        for (int i = 0; i < gears.Count; i++)
        {
            if (gears[i].speedCap > car.linearVelocity.magnitude)
                return i;
        }

        return 0;
    }
}
