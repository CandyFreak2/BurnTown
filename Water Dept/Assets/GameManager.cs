using UnityEngine;

public class GameManager : MonoBehaviour
{
    //internal timescale for worldObjects
    static private float timescale = 1f;
    static public float TimeScale
    {
        get { return timescale; }
        set { timescale = Mathf.Clamp(value, 0, Mathf.Infinity); }
    }
}
