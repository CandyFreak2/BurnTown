using UnityEngine;

public class TruckKillzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("try destroy stuff");
        Burnable burnableToDestroy = other.GetComponent<Burnable>();

        if (burnableToDestroy != null )
        {
            Debug.Log("Stuff destroyed");
            burnableToDestroy.DestroyObjectImpact(true);
        }
    }
}
