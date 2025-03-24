using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class Firefighter : NetworkBehaviour {
    public enum PlayerState {
        LOBBY,
        CUTSCENE,
        READY
    }

    public static readonly List<Firefighter> players = new();
    public static Action<Firefighter> PlayerJoined, PlayerLeft, PlayerChanged;

    public static Firefighter Local;

    [Networked] public NetworkBool IsReady { get; set; }
    [Networked] public NetworkBool HasFinished { get; set; } // End Goal
    [Networked] public PlayerState GameState { get; set; }
    [Networked] public int ID { get; set; }

    public bool IsLeader => Object != null && Object.IsValid && Object.HasStateAuthority;

    public override void Spawned() {
        base.Spawned();

        if (Object.HasInputAuthority) {
            Local = this;

            PlayerChanged?.Invoke(this);
            // Add way to change player's textures
        }

        players.Add(this);
        PlayerJoined?.Invoke(this);

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeReadyState(NetworkBool state) {
        Debug.Log($"Setting {Object.Name} ready state to {state}");
        IsReady = state;
    }

    public static void RemovePlayer(NetworkRunner runner, PlayerRef p) {
        var player = players.FirstOrDefault(i => i.Object.InputAuthority == p);

        if (player != null) {
            players.Remove(player);
            runner.Despawn(player.Object);
        }
    }

    private static void OnStateChanged(Firefighter changed) => PlayerChanged?.Invoke(changed);
    
    private void OnDisable() {
        PlayerLeft?.Invoke(this);
        players.Remove(this);
    }
}
