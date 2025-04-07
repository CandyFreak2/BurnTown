using Fusion.Addons.SimpleKCC;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _playerAnimator;
    private SimpleKCC _playerKCC;
    private PlayerMovement _playerScript;

    private float playerSpeed;
    private bool holdingFlamer;
    private bool holdingBarrel;
    private bool stunned;

    public float manualSpeedTweak = 1f;

    public Transform flamerGeo;
    public Transform flamerGeoOverwrite;
    public float flamerOffsetTweak;

    private void Awake()
    {
        _playerAnimator = GetComponentInParent<Animator>();
        _playerKCC = GetComponentInParent<SimpleKCC>();
        _playerScript = GetComponentInParent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _playerScript.onThrowBarrel += ThrowBarrel;
    }

    private void OnDisable()
    {
        _playerScript.onThrowBarrel -= ThrowBarrel;
    }

    private void Update()
    {
        //Set to default
        playerSpeed = 0;
        holdingFlamer = false;
        holdingBarrel = false;
        stunned = false;

        //Get current values
        playerSpeed = (_playerKCC.RealSpeed / _playerScript.maxSpeed) * manualSpeedTweak;
        stunned = _playerScript.IsStunned;

        switch(_playerScript.HeldInteractable)
        {
            case InteractableType.Flamethrower:
                holdingFlamer = true; 
                break;
            case InteractableType.Barrel:
                holdingBarrel = true;
                break;
        }

        //Assign values
        _playerAnimator.SetFloat("Speed", playerSpeed);
        _playerAnimator.SetBool("Holding Flamer", holdingFlamer);
        _playerAnimator.SetBool("Holding Barrel", holdingBarrel);
        _playerAnimator.SetBool("Stunned", stunned);

        
    }

    void ThrowBarrel()
    {
        _playerAnimator.SetTrigger("Throw");
        holdingBarrel = false; //idk if this line does anything
    }

    public void FootStepTrigger()
    {
        AudioPlayer.PlaySFX(_playerScript.sfxProfile.footstep, transform.position, 0.2f);
    }

    private void LateUpdate()
    {
        if(holdingFlamer && playerSpeed > 0.1f)
        {
            flamerGeoOverwrite.position = flamerGeo.position + flamerGeo.parent.parent.right * flamerOffsetTweak;
            flamerGeoOverwrite.rotation = flamerGeo.rotation;

            flamerGeoOverwrite.GetComponent<MeshRenderer>().enabled = true;
            flamerGeo.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            flamerGeoOverwrite.position = flamerGeo.position + flamerGeo.parent.parent.right * flamerOffsetTweak;
            flamerGeoOverwrite.rotation = flamerGeo.rotation;

            flamerGeoOverwrite.GetComponent<MeshRenderer>().enabled = false;
        }

    }
}
