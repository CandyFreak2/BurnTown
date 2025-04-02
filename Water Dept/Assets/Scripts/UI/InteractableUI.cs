using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUI : NetworkBehaviour
{
    private GameObject flamethrowerUI;
    [SerializeField] private Animator flamethrowerIconAnim;

    private GameObject barrelUI;
    private Image barrelIcon;

    private GameObject firetruckUI;
    private Image firetruckIcon;

    private PlayerMovement playerMovement;

    private InputButton inputPressed;
    [SerializeField] private float popupDelay = 0.25f;

    [Header("Tween")]
    [SerializeField] float tweenDuration;
    [SerializeField] Vector3 tweenMagnatude;
    [SerializeField] int tweenVibrato;
    Sequence[] activeTweens;


    [Serializable]
    public class ButtonUI
    {
        public Sprite defaultSprite, pressedSprite;
    }
    public List<ButtonUI> buttonSprites;
    public static Dictionary<string, ButtonUI> spriteMap;
    

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
        InitSprites();
    }

    // Make dictionary of sprites so that accessing sprites will be more intuitive
    private void InitSprites()
    {
        spriteMap = new Dictionary<string, ButtonUI>();

        if (buttonSprites.Count >= 7)
        { 
            spriteMap.Add("AButton", buttonSprites[0]);
            spriteMap.Add("BButton", buttonSprites[1]);
            spriteMap.Add("XButton", buttonSprites[2]);
            spriteMap.Add("YButton", buttonSprites[3]);
            spriteMap.Add("RTButton", buttonSprites[4]);
            spriteMap.Add("LTButton", buttonSprites[5]);
            spriteMap.Add("BarrelIcon", buttonSprites[6]);
            spriteMap.Add("FiretruckIcon", buttonSprites[7]);
        }
        else
        {
            Debug.Log("Sprite list no long enough");
        }

        // Initialize ui image refrences
        barrelIcon = barrelUI.transform.Find("BarrelIcon")?.GetComponent<Image>();
        firetruckIcon = firetruckUI.transform.Find("FiretruckIcon")?.GetComponent<Image>();
    }

    // Delay updating ui so that playermovement data gets updated first then ui
    private void DelayUpdateUI(InputButton _input)
    {
        inputPressed = _input;

        if (inputPressed == InputButton.ShootDown || inputPressed == InputButton.ShootUp || inputPressed == InputButton.BrakeDown || inputPressed == InputButton.BrakeUp)
        {
            Invoke("UpdateUI", popupDelay);
        }
        else
        {
            Invoke("UpdateUI", popupDelay);

            // If trigger not pressed tween ui
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

        if (!playerMovement.enabled) //Should prolly change this check with stun disabling playermovement too
        {
            firetruckUI.SetActive(true);
            UpdateFiretruckUI();
        }
        else if (playerMovement.carriedObject == null)
        {
            DeactivateAllUI();
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Flamethrower)
        {
            flamethrowerUI.SetActive(true);
            UpdateFlamethrowerUI();
        }
        else if (playerMovement.carriedObject.interactableType == InteractableType.Barrel)
        {
            barrelUI.SetActive(true);
            UpdateBarrelUI();
        }
        else
        {
            DeactivateAllUI();
        }
    }

    private void DeactivateAllUI()
    {
        // Reset button sprites

        flamethrowerUI.SetActive(false);
        barrelUI.SetActive(false);
        firetruckUI.SetActive(false);
    }

    private void UpdateFlamethrowerUI()
    {
        // Toggle flamethrower sprite animation
        if (inputPressed == InputButton.ShootDown)
        {
            flamethrowerIconAnim.SetBool("isShooting", true);
        }

        else if (inputPressed == InputButton.ShootUp)
        {
            flamethrowerIconAnim.SetBool("isShooting", false);
        }
    }

    private void UpdateBarrelUI()
    {
        if (inputPressed == InputButton.Drop)
        {
            if (barrelIcon.sprite == spriteMap["BarrelIcon"].defaultSprite)
            {
                barrelIcon.sprite = spriteMap["BarrelIcon"].pressedSprite;
            }
            else
            {
                barrelIcon.sprite = spriteMap["BarrelIcon"].defaultSprite;
            }
        }
    }

    private void UpdateFiretruckUI()
    {
        // Toggle Firetuck icon sprite
        if (inputPressed == InputButton.ShootDown || inputPressed == InputButton.BrakeDown)
        {
            firetruckIcon.sprite = spriteMap["FiretruckIcon"].pressedSprite;
        }

        else if (inputPressed == InputButton.ShootUp || inputPressed == InputButton.BrakeUp)
        {
            firetruckIcon.sprite = spriteMap["FiretruckIcon"].defaultSprite;
        }
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetPlayerState(RpcInfo info = default)
    {
        playerMovement = Runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
    }
}
