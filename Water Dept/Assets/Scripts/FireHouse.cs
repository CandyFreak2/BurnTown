using UnityEngine;
using System.Collections;

public class FireHouse : MonoBehaviour
{
    [SerializeField] GameObject water;
    [SerializeField] public float waterForce = 10f;
   
    public void SprayWater()
    {
        water.SetActive(true);
    }

    public IEnumerator StopSprayingWater()
    {
        yield return new WaitForSeconds(0.5f);
        water.SetActive(false);
    }
}
