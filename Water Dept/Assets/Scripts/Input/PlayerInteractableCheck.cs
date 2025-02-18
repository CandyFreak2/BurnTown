using Fusion;
using UnityEngine;

public class PlayerInteractableCheck : MonoBehaviour
{
    [SerializeField] PlayerMovement playerController;

    private void OnCollisionEnter(Collision collision)
    {
        if (!playerController.IsHolding() && collision.gameObject.tag == "FireHoseHead")
        {
            playerController.PickUpObject(collision.gameObject.GetComponent<Interactable>());
        }
    }
}
