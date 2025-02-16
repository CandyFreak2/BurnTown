using UnityEngine;

public class TPlayerInputAction
{
    public Vector2 MoveDirection;
    public Vector2 LookDelta;

    public TPlayerInputAction()
    {
        MoveDirection = Vector2.zero;
        LookDelta = Vector2.zero;
    }

    public TPlayerInputAction(TPlayerInputAction action)
    {
        MoveDirection = action.MoveDirection;
        LookDelta = action.LookDelta;
    }
}
