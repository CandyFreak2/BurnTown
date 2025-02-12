using UnityEngine;

public class PlayerInteractableCheck : MonoBehaviour
{
    [SerializeField] PlayerMovement playerController;

    private void OnCollisionEnter(Collision collision)
    {
        if (!playerController.isHolding && collision.gameObject.tag == "FireHoseHead")
        {
            playerController.PickUpHose(collision.gameObject.transform.parent.gameObject, collision.gameObject);
        }
    }
}
