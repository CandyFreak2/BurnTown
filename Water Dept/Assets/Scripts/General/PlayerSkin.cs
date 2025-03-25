using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSkin : NetworkBehaviour {
    [Header("Models")]
    [SerializeField] List<SkinnedMeshRenderer> playerSkins;
    [SerializeField] List<MeshRenderer> playerModels;

    [Header("Skins")]
    [SerializeField] List<Material> skins;

    [Header("Debug")]
    [SerializeField] bool debugTrigger;


    public override void Spawned() {
        SetPlayerSkin(Runner.LocalPlayer.AsIndex - 1);
    }

    /// <summary> Set's the player's skin to a given material. Also updates the player on other people's screens using an RPC call </summary>
    public void SetPlayerSkin(int index) {
        index = Mathf.Clamp(index, 0, skins.Count - 1);
        if (Object.HasInputAuthority && !debugTrigger) {
            //Tell host to issue the command
            RPC_SendPlayerSkin(index);
            debugTrigger = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_SendPlayerSkin(int index, RpcInfo info = default) {
        //Host relays the command to all clients
        RPC_RealyPlayerSkin(index, info.Source);
    }

    //Recieve the host's command
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_RealyPlayerSkin(int index, PlayerRef messageSource) {
        if (messageSource == Runner.LocalPlayer) {
            //You have set your skin to somthing new
            OverwriteSkin(index);
        } else {
            //Other player has set their skin to somthing new
            OverwriteSkin(index);
        }
    }

    //where the skin actually gets set
    private void OverwriteSkin(int index) {
        foreach (SkinnedMeshRenderer renderer in playerSkins) {
            renderer.material = skins[index];
        }

        foreach (MeshRenderer renderer in playerModels) {
            renderer.material = skins[index];
        }
    }
}
