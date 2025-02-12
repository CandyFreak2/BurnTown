using UnityEngine;

public class BarrelInfo : MonoBehaviour
{
    private BarrelInfo tail;
    private BarrelInfo head;

    private void explode()
    {
        Debug.Log("Test");
        //coroutine

        if (head != null)
        {
            head.explode();
        }
        else if (tail != null)
        {
            
            tail.explode();
        }
    }


}

