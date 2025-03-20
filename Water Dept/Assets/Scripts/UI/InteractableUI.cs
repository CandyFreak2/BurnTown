using DG.Tweening;
using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class InteractableUI : NetworkBehaviour
{
    private GameObject flamethrowerUI;
    private GameObject barrelUI;
    private GameObject firetruckUI;

    private PlayerMovement playerMovement;

    private bool isAttackPressed;
    [SerializeField] private float popupDelay = 0.25f;

    [Header("Tween")]
    [SerializeField] float tweenDuration;
    [SerializeField] Vector3 tweenMagnatude;
    [SerializeField] int tweenVibrato;
    Tween activeTween;

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
    private void DelayUpdateUI(bool _isAttackPressed)
    {
        isAttackPressed = _isAttackPressed;

        if (isAttackPressed)
        {
            Invoke("UpdateUI", popupDelay);
        }
        else
        {
            if (activeTween != null) activeTween.Complete();
            activeTween = flamethrowerUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
            activeTween = barrelUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
            activeTween = firetruckUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);

            Invoke("UpdateUI", popupDelay);
        }
    }

    private void UpdateUI()
    {
        RPC_SetPlayerState();

        
        if (!playerMovement.enabled)
        {
            firetruckUI.SetActive(true);

            if (activeTween != null) activeTween.Complete();
            activeTween = firetruckUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
        }
        else if (playerMovement.carriedObject == null)
        {
            DeactivateAllUI();
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Flamethrower)
        {
            flamethrowerUI.SetActive(true);

            if (activeTween != null) activeTween.Complete();

            // Wont tween every time it shoots
            if (!isAttackPressed)
            {
                activeTween = flamethrowerUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
            }
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Barrel)
        {
            barrelUI.SetActive(true);

            if (activeTween != null) activeTween.Complete();
            activeTween = barrelUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato);
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
