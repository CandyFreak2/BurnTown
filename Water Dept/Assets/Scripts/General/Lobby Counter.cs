using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyCounter : MonoBehaviour
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
        EventManager.onStartZoneTrigger += RPC_SignalStart;
    }

    private void OnDisable()
    {
        EventManager.onStartZoneTrigger -= RPC_SignalStart;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SignalStart() => RPC_RelayStart();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayStart()
    {
        triggerStart?.Invoke();
    }
}
