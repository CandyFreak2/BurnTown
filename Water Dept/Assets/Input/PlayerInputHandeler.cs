using System;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class PlayerInputHandeler : MonoBehaviour
{
    InputSystem_Actions inputSystem;

    Vector2 moveDirection;
    Vector2 lookDelta;

    public PlayerInputAction input = new PlayerInputAction();
    public Action<PlayerInputAction> onInputSignal;

    private void Awake()
    {
        inputSystem = new InputSystem_Actions();

        //get input and leave it hanging
        inputSystem.Player.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        inputSystem.Player.Look.performed += ctx => lookDelta = ctx.ReadValue<Vector2>();

        //clear input
        inputSystem.Player.Move.canceled += ctx => moveDirection = ctx.ReadValue<Vector2>();
    }

    private void OnEnable() //restarts controls when needed
    {
        inputSystem.Enable();
    }

    private void OnDisable() //prevents controls running in editor and making junk
    {
        inputSystem.Disable();

        moveDirection = Vector2.zero;
        lookDelta = Vector2.zero;
    }

    private void Start()
    {
        onInputSignal?.Invoke(new PlayerInputAction(input));
    }

    private void Update()
    {
        //load inputs into the package
        input.MoveDirection = moveDirection;
        
        if(lookDelta.magnitude > 0) 
            input.LookDelta = lookDelta;

        //send out a copy of this frame's input to all listeners?
        onInputSignal?.Invoke(new PlayerInputAction(input));
    }
}
