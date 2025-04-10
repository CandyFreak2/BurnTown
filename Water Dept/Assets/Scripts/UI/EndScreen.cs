using DG.Tweening;
using Fusion;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EndScreen : NetworkBehaviour
{
    [SerializeField] PlayableDirector endTimeline;
    [SerializeField] TextMeshProUGUI _Score;
    [SerializeField] TextMeshProUGUI _Time;
    [SerializeField] GameObject defaultButton;

    [SerializeField] float[] scoreGrades = {0f, 10000f, 30000f};
    [SerializeField] float[] timeGrades = {9999f, 360f, 120f};

    [SerializeField] List<GameObject> scoreIcon;
    [SerializeField] List<GameObject> timeIcon;

    [SerializeField] float tallyTimeMaxScore;
    [SerializeField] float tallyTimeMinScore;
    [SerializeField] float tallyTimeMax;
    [SerializeField] float tallyTimeMin;


    [SerializeField] List<AudioClip> tickSfx;
    [SerializeField] List<AudioClip> tallyFinishedSfx;

    Coroutine corutine;
    [SerializeField] UnityEvent onTallyComplete;

    public void TallyScore(float score, float time)
    {
        foreach (var item in scoreIcon) 
            item.SetActive(false);
        foreach (var item in timeIcon)
            item.SetActive(false);

        if (corutine == null)
            corutine = StartCoroutine(RunTally(score, time));
    }

    IEnumerator RunTally(float score, float time)
    {
        float delay = 0.02f;
        WaitForSeconds tick = new WaitForSeconds(delay);
        float magnitude = Mathf.InverseLerp(tallyTimeMinScore, tallyTimeMaxScore, score);
        float duration = Mathf.Lerp(tallyTimeMin, tallyTimeMax, magnitude);
        float timer = 0;

        while (timer < duration)
        {
            _Score.text = "$ " + (score * (timer / duration));
            _Time.text = FormatTime(time * (timer / duration));

            AudioPlayer.PlaySFX(tickSfx, Camera.main.transform.position, 0.2f, 1f);

            yield return tick;
            timer += delay;
        }

        _Score.text = "$ " + score;
        _Time.text = FormatTime(time);

        AudioPlayer.PlaySFX(tallyFinishedSfx, Camera.main.transform.position, 0f, 0.5f);

        _Score.transform.DOShakeRotation(.5f, 60 * magnitude, 20);
        _Score.transform.DOShakePosition(.5f, .5f * magnitude, 20);

        _Time.transform.DOShakeRotation(.5f, 60 * magnitude, 20);
        _Time.transform.DOShakePosition(.5f, .5f * magnitude, 20);

        int scoreIndex = 0;
        int timeIndex = 0;

        for (int i = 0; i < scoreGrades.Length; i++)
            if(score > scoreGrades[i])
                scoreIndex = i;

        for(int i = 0; i < timeGrades.Length; i++)
            if(time < timeGrades[i])
                timeIndex = i;

        scoreIcon[scoreIndex].SetActive(true);
        timeIcon[timeIndex].SetActive(true);

        scoreIcon[scoreIndex].transform.DOPunchScale(Vector3.one * 0.8f, 0.3f, 1);
        timeIcon[timeIndex].transform.DOPunchScale(Vector3.one * 0.8f, 0.3f, 1);

        EventSystem.current.SetSelectedGameObject(defaultButton);
        onTallyComplete?.Invoke();
    }

    //Assign to links and stuff
    private void OnEnable()
    {
        //EventManager.onEndZoneTrigger += TriggerEndScreen; 
    }

    private void OnDisable()
    {
        //EventManager.onEndZoneTrigger -= TriggerEndScreen;
    }

    //bleeeghhhhh networking :(

    public void TriggerEndScreen(EndConditions conditions)
    {
        Debug.Log("END SCREEN TRIGGER");
        endTimeline.Play();
    }

    public void TriggerTally()
    {
        if (Runner.IsSceneAuthority)
            RPC_TallyScoreSignal(PlayerPrefs.GetInt("N_FinalScore"), PlayerPrefs.GetFloat("N_FinalTime")); //temp time

        //TallyScore(PlayerPrefs.GetInt("N_FinalScore"), 156f);
    }

    public string FormatTime(float time) {
        if (time < 0) {
            time = 0;
        }
        float minutes = Mathf.Floor(time / 60);
        float seconds = Mathf.Floor(time % 60);

        return string.Format("{0:00} min - {1:00} sec", minutes, seconds);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_TallyScoreSignal(float score, float time) => RPC_TallyScoreRelay(score, time);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_TallyScoreRelay(float score, float time) => TallyScore(score, time);

}
