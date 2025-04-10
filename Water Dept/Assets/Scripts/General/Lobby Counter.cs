using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyCounter : NetworkBehaviour
{
    [SerializeField] StartZone linkedZone;
    [SerializeField] TextMeshPro playerCount;
    [SerializeField] TextMeshPro countdown;
    [SerializeField] TextMeshPro lobbyCode;
    string defaultText;
    private bool isSpawned = false;

    [SerializeField] UnityEvent triggerStart;

    private void Awake()
    {
        defaultText = countdown.text;
    }

    public override void Spawned()
    {
        isSpawned = true;
    }

    private void FixedUpdate()
    {
        if (isSpawned)
        {
            playerCount.text = linkedZone.GetCurrentPlayers + "/" + linkedZone.GetMaxPlayers;
            countdown.text = linkedZone.GetCurrentPlayers == linkedZone.GetMaxPlayers ? "Start in:\n" + (linkedZone.GetTimeLeft / 10f) * 10f : defaultText;
            lobbyCode.text = linkedZone.GetLobbyCode;
        }
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
