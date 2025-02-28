using UnityEngine;
using Fusion;

public enum InteractableType
{
    Flamethrower,
    Barrel
}

public class Interactable : NetworkBehaviour
{
    private bool isSpawned;

    [Networked]
    private NetworkBool isPickedUp { get; set; }

    [Networked]
    private NetworkId carrierId { get; set; }

    private Rigidbody rb;
    private Collider interactableCollider;

    [SerializeField]
    private Vector3 holdOffset = new Vector3(1.0f, 0.5f, 0.0f);

    [SerializeField]
    public InteractableType interactableType;

    private void Awake()
    {
        isSpawned = false;
        rb = GetComponent<Rigidbody>();
        interactableCollider = GetComponent<Collider>();
    }

    public override void Spawned()
    {
        if (/*HasStateAuthority*/ true)
        {
            isSpawned = true;
            isPickedUp = false;
            carrierId = default;
        }
    }

    private void FixedUpdate()
    {
        if (isSpawned && isPickedUp /*&& HasStateAuthority*/)
        {
            NetworkObject carrier = Runner.FindObject(carrierId);

            // Update position of interactable to player
            if (carrier != null)
            {
                // FIX THIS - SHOULD NOT BE CHECKING FOR NAME OR INDEX OF CHILD
                // COMPARE TAG OF CHILD OR REFERENCE POSITION OF OBJECT
                Transform handtransform = carrier.transform.GetChild(0);

                transform.position = handtransform.position + handtransform.TransformDirection(holdOffset);
                transform.rotation = handtransform.rotation;
            }
            else // drop if disconnects
            {
                Debug.Log("Drop but not want to");
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
