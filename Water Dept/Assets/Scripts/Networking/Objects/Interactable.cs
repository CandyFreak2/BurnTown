using UnityEngine;
using Fusion;

public class Interactable : NetworkBehaviour
{
    [Networked]
    private NetworkBool isPickedUp { get; set; }

    [Networked]
    private NetworkId carrierId { get; set; }

    private Rigidbody rb;
    private Collider interactableCollider;

    [SerializeField]
    private Vector3 holdOffset = new Vector3(1.0f, 0.5f, 0.0f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactableCollider = GetComponent<Collider>();
    }

    public override void Spawned()
    {
        if (/*HasStateAuthority*/ true)
        {
            isPickedUp = false;
            carrierId = default;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (isPickedUp /*&& HasStateAuthority*/)
        {
            NetworkObject carrier = Runner.FindObject(carrierId);

            // Update position of interactable to player
            if (carrier != null)
            {
                transform.position = carrier.transform.position + carrier.transform.TransformDirection(holdOffset);
                transform.rotation = carrier.transform.rotation;
            }
            else // drop if disconnects
            {
                Drop();
            }
        }
    }

    public void PickUp(NetworkId _playerId)
    {
        if (/*HasStateAuthority &&*/ !isPickedUp)
        {
            isPickedUp = true;
            carrierId = _playerId;

            // Disable physics while held
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            interactableCollider.enabled = false;
        }
    }

    public void Drop()
    {
        if (/*HasStateAuthority &&*/ isPickedUp)
        {
            Debug.Log("try Drop in interactable :)");
            isPickedUp = false;
            carrierId = default;

            // Disable physics while held
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            interactableCollider.enabled = true;
        }
    }

    public bool CanBePickedUp()
    {
        return !isPickedUp;
    }

    public NetworkId GetCarrierId()
    {
        return carrierId;
    }
}
