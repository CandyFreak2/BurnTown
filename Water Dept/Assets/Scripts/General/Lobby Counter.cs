using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyCounter : NetworkBehaviour
{
    [SerializeField] StartZone linkedZone;
    [SerializeField] TextMeshPro playerCount;
    [SerializeField] TextMeshPro lobbyCode;
    string defaultText;
    private bool isSpawned = false;

    [SerializeField] UnityEvent triggerStart;

    public override void Spawned()
    {
        isSpawned = true;
    }

    private void FixedUpdate()
    {
        if (isSpawned)
        {
            playerCount.text = linkedZone.GetCurrentPlayers + "/" + linkedZone.GetMaxPlayers;
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
