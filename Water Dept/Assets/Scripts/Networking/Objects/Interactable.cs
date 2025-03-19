using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

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

//using UnityEngine;
//using Fusion;
//using Fusion.Addons.Physics;

//public enum InteractableType
//{
//    Flamethrower,
//    Barrel
//}

//public class Interactable : NetworkBehaviour
//{
//    private bool isSpawned;

//    [Networked]
//    public NetworkBool isPickedUp { get; set; }

//    [Networked]
//    public NetworkId carrierId { get; set; }

//    private Rigidbody rb;
//    private NetworkRigidbody3D networkRB;
//    private Collider interactableCollider;

//    [SerializeField]
//    private Vector3 holdOffset = new Vector3(1.0f, 0.5f, 0.0f);

//    [SerializeField]
//    public InteractableType interactableType;

//    private void Awake()
//    {
//        isSpawned = false;
//        rb = GetComponent<Rigidbody>();
//        interactableCollider = GetComponent<Collider>();
//    }

//    public override void Spawned()
//    {
//        if (/*HasStateAuthority*/ true)
//        {
//            isSpawned = true;
//            isPickedUp = false;
//            carrierId = default;
//            networkRB = GetComponent<NetworkRigidbody3D>();
//        }
//    }

//    private void FixedUpdate()
//    {
//        if (isSpawned && isPickedUp /*&& HasStateAuthority*/)
//        {
//            NetworkObject carrier = Runner.FindObject(carrierId);

//            // Update position of interactable to player
//            if (carrier != null)
//            {
//                Transform handtransform = carrier.transform.Find("PlayerModelObj");

//                transform.position = handtransform.position + handtransform.TransformDirection(holdOffset);
//                transform.rotation = handtransform.rotation;
//            }
//            else // drop if disconnects
//            {
//                Debug.Log("Drop but not want to");
//                Drop();
//            }
//        }
//    }

//    public void PickUp(NetworkId _playerId)
//    {
//        if (/*HasStateAuthority &&*/ !isPickedUp)
//        {

//            isPickedUp = true;
//            carrierId = _playerId;

//            // Disable physics while held
//            RPC_RequestPickUp(Object.Id, _playerId);
//            rb.isKinematic = true;
//            rb.constraints = RigidbodyConstraints.FreezeAll;
//            interactableCollider.enabled = false;
//            networkRB.enabled = false;
//        }
//    }

//    public void Drop()
//    {
//        if (/*HasStateAuthority &&*/ isPickedUp)
//        {

//            isPickedUp = false;
//            carrierId = default;

//            // Disable physics while held
//            RPC_RequestDrop(Object.Id);
//            rb.isKinematic = false;
//            rb.constraints = RigidbodyConstraints.None;
//            interactableCollider.enabled = true;
//            networkRB.enabled = true;
//        }
//    }

//    [Rpc(RpcSources.All, RpcTargets.All)] // Run by all
//    public void RPC_RequestPickUp(NetworkId _interactableId, NetworkId _playerId)
//    {
//        Interactable interactable = Runner.FindObject(_interactableId).GetComponent<Interactable>();

//        if (/*HasStateAuthority &&*/ interactable != null)
//        {
//            Debug.Log("Yoooo in rpc");
//            //interactable.isPickedUp = true;
//            //interactable.carrierId = _playerId;

//            // Disable physics while held
//            interactable.rb.isKinematic = true;
//            interactable.rb.constraints = RigidbodyConstraints.FreezeAll;
//            interactable.interactableCollider.enabled = false;
//            interactable.networkRB.enabled = false;
//        }
//    }

//    [Rpc(RpcSources.All, RpcTargets.All)] // Run by all
//    public void RPC_RequestDrop(NetworkId _interactableId)
//    {
//        Interactable interactable = Runner.FindObject(_interactableId).GetComponent<Interactable>();

//        if (/*HasStateAuthority &&*/ interactable != null)
//        {
//            //interactable.isPickedUp = false;
//            //interactable.carrierId = default;

//            // Disable physics while held
//            interactable.rb.isKinematic = false;
//            interactable.rb.constraints = RigidbodyConstraints.None;
//            interactable.interactableCollider.enabled = true;
//            interactable.networkRB.enabled = true;
//        }
//    }

//    public bool CanBePickedUp()
//    {
//        return !isPickedUp;
//    }

//    public NetworkId GetCarrierId()
//    {
//        return carrierId;
//    }
//}
