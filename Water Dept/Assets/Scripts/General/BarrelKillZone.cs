using UnityEngine;

public class BarrelKillZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Barrel")
        {
            collision.gameObject.GetComponent<BarrelInfo>().Explode();
        }
        Debug.Log(collision.gameObject);
    }
}
