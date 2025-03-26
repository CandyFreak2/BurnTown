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
    Sequence[] activeTweens;

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

        activeTweens = new Sequence[3];
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
            Invoke("UpdateUI", popupDelay);

            for (int i = 0; i < activeTweens.Length; i++)
            {
                activeTweens[i].Kill();
            }

            activeTweens[0] = DOTween.Sequence()
                .Append(flamethrowerUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato))
                .Append(flamethrowerUI.transform.DOScale(Vector3.one, 0.1f));

            activeTweens[1] = DOTween.Sequence()
                .Append(barrelUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato))
                .Append(barrelUI.transform.DOScale(Vector3.one, 0.1f));

            activeTweens[2] = DOTween.Sequence()
                .Append(firetruckUI.transform.DOShakeScale(tweenDuration, tweenMagnatude, tweenVibrato))
                .Append(firetruckUI.transform.DOScale(Vector3.one, 0.1f));

            for (int i = 0; i < activeTweens.Length; i++)
            {
                activeTweens[i].PlayForward();
            }
        }
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
