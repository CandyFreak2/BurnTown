using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class InteractableUI : NetworkBehaviour
{
    [SerializeField] private GameObject flamethrowerUI;
    [SerializeField] private GameObject barrelUI;
    [SerializeField] private GameObject firetruckUI;

    PlayerMovement playerMovement;

    private void OnEnable()
    {
        UIInputHandler.onActionInteract += DelayUpdateUI;
    }

    private void OnDisable()
    {
        UIInputHandler.onActionInteract -= DelayUpdateUI;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flamethrowerUI = UIManager.Instance.GetFlamethrowerUI();
        barrelUI = UIManager.Instance.GetBarrelUI();
        firetruckUI = UIManager.Instance.GetFiretruckUI();
    }

    // Delay updating ui so that playermovement data gets updated first then ui
    private void DelayUpdateUI()
    {
        Invoke("UpdateUI", 0.25f);
    }

    private void UpdateUI()
    {
        RPC_SetPlayerState();

        
        if (!playerMovement.enabled)
        {
            firetruckUI.SetActive(true);
        }
        else if (playerMovement.carriedObject == null)
        {
            DeactivateAllUI();
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Flamethrower)
        {
            flamethrowerUI.SetActive(true);
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Barrel)
        {
            barrelUI.SetActive(true);
        }
        else
        {
            DeactivateAllUI();
        }
    }

    private void DeactivateAllUI()
    {
        flamethrowerUI.SetActive(false);
        barrelUI.SetActive(false);
        firetruckUI.SetActive(false);
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetPlayerState(RpcInfo info = default)
    {
        playerMovement = Runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
    }
}
