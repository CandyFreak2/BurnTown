using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class InteractableUI : NetworkBehaviour
{
    private GameObject flamethrowerUI;
    private Image flamethrowerShoot, flamethrowerDrop;

    private GameObject barrelUI;
    private GameObject firetruckUI;

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
    private Dictionary<string, ButtonUI> spriteMap;
    

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

        if (buttonSprites.Count >= 6)
        { 
            spriteMap.Add("AButton", buttonSprites[0]);
            spriteMap.Add("BButton", buttonSprites[1]);
            spriteMap.Add("XButton", buttonSprites[2]);
            spriteMap.Add("YButton", buttonSprites[3]);
            spriteMap.Add("RTButton", buttonSprites[4]);
            spriteMap.Add("LTButton", buttonSprites[5]);
        }
        else
        {
            Debug.Log("Sprite list no long enough");
        }

        // Initialize ui image refrences
        flamethrowerShoot = flamethrowerUI.transform.Find("FlamethrowerShoot")?.GetComponent<Image>();
        flamethrowerDrop = flamethrowerUI.transform.Find("FlamethrowerDrop")?.GetComponent<Image>();
    }

    // Delay updating ui so that playermovement data gets updated first then ui
    private void DelayUpdateUI(InputButton _input)
    {
        inputPressed = _input;

        if (inputPressed == InputButton.Shoot)
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
            UpdateFlamethrowerUI();
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
        // Reset button sprites
        flamethrowerShoot.sprite = spriteMap["RTButton"].defaultSprite;
        flamethrowerDrop.sprite = spriteMap["BButton"].defaultSprite;

        flamethrowerUI.SetActive(false);
        barrelUI.SetActive(false);
        firetruckUI.SetActive(false);
    }

    private void UpdateFlamethrowerUI()
    {
        // Toggle trigger sprite
        if (inputPressed == InputButton.Shoot)
        {
            if (flamethrowerShoot.sprite == spriteMap["RTButton"].defaultSprite)
            {
                flamethrowerShoot.sprite = spriteMap["RTButton"].pressedSprite;
            }
            else
            {
                flamethrowerShoot.sprite = spriteMap["RTButton"].defaultSprite;
            }

            // Could do on fire icon in here just not sure how to do that animation
        }

        // Toggle drop sprite
        else if (inputPressed == InputButton.Drop)
        {
            if (flamethrowerDrop.sprite == spriteMap["BButton"].defaultSprite)
            {
                flamethrowerDrop.sprite = spriteMap["BButton"].pressedSprite;
            }
            else
            {
                flamethrowerDrop.sprite = spriteMap["BButton"].defaultSprite;
            }
        }
    }

    [Rpc(HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetPlayerState(RpcInfo info = default)
    {
        playerMovement = Runner.GetPlayerObject(info.Source).GetComponent<PlayerMovement>();
    }
}
