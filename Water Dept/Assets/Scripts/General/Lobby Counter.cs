using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyCounter : NetworkBehaviour
{
    [SerializeField] StartZone linkedZone;
    [SerializeField] TextMeshPro playerCount;
    [SerializeField] TextMeshPro countdown;
    string defaultText;

    [SerializeField] UnityEvent triggerStart;

    private void Awake()
    {
        defaultText = countdown.text;
    }

    private void FixedUpdate()
    {
        playerCount.text = linkedZone.GetCurrentPlayers + "/" + linkedZone.GetMaxPlayers;
        countdown.text = linkedZone.GetCurrentPlayers == linkedZone.GetMaxPlayers ? "Start in:\n" + (linkedZone.GetTimeLeft / 1000f) * 1000f : defaultText;
    }

    private void OnEnable()
    {
        EventManager.onStartZoneTrigger += TriggerStart;
    }

    private void OnDisable()
    {
        EventManager.onStartZoneTrigger -= TriggerStart;
    }

    public void TriggerStart()
    {
        triggerStart?.Invoke();
    }
}
