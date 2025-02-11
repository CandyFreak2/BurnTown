using UnityEngine;

public class PlayerInteractableCheck : MonoBehaviour
{
    [SerializeField] PlayerMovement playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("Yo u triggered");
    //    if (!playerController.isHolding && other.gameObject.tag == "FireHoseHead")
    //    {
    //        playerController.PickUpHose(other.gameObject);
    //    }
    //}

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Yo u collided");
        if (!playerController.isHolding && collision.gameObject.tag == "FireHoseHead")
        {
            playerController.PickUpHose(collision.gameObject.transform.parent.gameObject, collision.gameObject);
        }
    }
}
