using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TruckKillzone : MonoBehaviour
{
    public bool superCrush = false;
    [SerializeField] float rumbleMultiplier = 3.0f;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("try destroy stuff");
        Burnable burnableToDestroy = other.GetComponent<Burnable>();

        if (burnableToDestroy != null )
        {
            //Debug.Log("Stuff destroyed");
            StartCoroutine(RumbleController());
            burnableToDestroy.DestroyObjectImpact(superCrush);
        }
    }

    private IEnumerator RumbleController()
    {
        Gamepad.current.SetMotorSpeeds(0.25f * rumbleMultiplier, 0.75f * rumbleMultiplier);

        yield return new WaitForSeconds(1.5f);

        Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
    }
}
