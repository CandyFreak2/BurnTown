using Fusion;
using UnityEngine;

public class tempBarrel : NetworkBehaviour {
    [SerializeField] private GameObject placeholderPrefab;
    [SerializeField] private GameObject barrelPrefab;

    public bool isSchemOut = false;
    public bool isThrowing = false;
    private GameObject schematic;

    [SerializeField] private float throwForce = 450;

    [Tooltip("Distance barrel is placed from player")]
    [SerializeField] public int placeDistance = 3;

    private Transform orientation;

    public override void Spawned() {

    }

    void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.E) && isSchemOut) {
            PlaceBarrel();
            if (isSchemOut) {
                isSchemOut = false;
                Destroy(schematic);
            }
        }


        if (isSchemOut) {
            SchemBarrel();
        }
    }

    //queue schematic if barrel is nearby
    public void BarrelSchematics(Transform playerOrientation) {
        if (!isSchemOut) {
            isSchemOut = true;
            schematic = Instantiate(placeholderPrefab);
            //schematic = Runner.Spawn(placeholderPrefab).gameObject;

            orientation = playerOrientation;
        }
    }

    public void BarrelThrow(Transform playerOrientation) {
        if (!Runner.IsClient) {
            if (isSchemOut) {
                isThrowing = true;

                NetworkObject bar = Runner.Spawn(barrelPrefab);

                bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) + playerOrientation.forward * placeDistance;
                bar.GetComponent<BarrelInfo>().isThrown = true;
                bar.GetComponent<Rigidbody>().AddForce((orientation.forward + new Vector3(0, 1, 0)) * throwForce);

                SnapPosition(bar.gameObject);

                if (isSchemOut) {
                    isSchemOut = false;
                    Destroy(schematic);
                }

                //prevents player from throwign on stacks
                isThrowing = false;

            }
        } else if (Runner.IsClient) {
            RPC_RequestBarrelThrow(playerOrientation.forward, Object.Id);
        }

    }

    public void KillSchematic() {
        isSchemOut = false;
        if (schematic != null) {
            Destroy(schematic);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestBarrelThrow(Vector3 playerOrientation, NetworkId b) {
        tempBarrel tBarr = Runner.FindObject(b).GetComponent<tempBarrel>();
        tBarr.isSchemOut = true;
        if (tBarr.isSchemOut) {


            tBarr.isThrowing = true;

            NetworkObject bar = Runner.Spawn(barrelPrefab);

            bar.transform.position = (tBarr.gameObject.GetComponent<Transform>().transform.position) + playerOrientation * placeDistance;
            bar.GetComponent<BarrelInfo>().isThrown = true;
            bar.GetComponent<Rigidbody>().AddForce((orientation.forward + new Vector3(0, 1, 0)) * throwForce);

            tBarr.SnapPosition(bar.gameObject);

            if (tBarr.isSchemOut) {
                tBarr.isSchemOut = false;
                Destroy(tBarr.schematic);
            }

            //prevents player from throwign on stacks
            tBarr.isThrowing = false;

        }
    }

    //gets repalced by existing pickup
    //private bool PickupBarrel()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, pickupDistance);
    //    foreach (Collider hit in colliders)
    //    {
    //        if(hit.tag == "Barrel")
    //        {
    //            Destroy(hit.gameObject);
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //hovers a schematic prefab in front of the player indicating where a barrel will be placed
    private void SchemBarrel() {
        isSchemOut = true;
        if (isSchemOut) {
            schematic.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
               orientation.forward * placeDistance;
        }

        SnapPosition(schematic);
    }

    //places a barrel prefab where the schematic was previously
    private void PlaceBarrel() {
        GameObject bar = Instantiate(barrelPrefab);

        bar.transform.position = (gameObject.GetComponent<Transform>().transform.position) +
                gameObject.GetComponent<Transform>().transform.forward * placeDistance;
        SnapPosition(bar);

        //sfx
        AudioPlayer.PlaySFX(bar.GetComponent<BarrelInfo>().placeSFX, bar.transform.position, 0.1f);
    }

    //assigns a barrels position to snap to the ground and/or a barrel nearby
    private void SnapPosition(GameObject obj) {
        RaycastHit hit;
        //casts ray from the sky down
        if (Physics.Raycast(obj.transform.position + new Vector3(0, 100f, 0), Vector3.down, out hit)) {
            //snaps barrel to ground
            if (hit.collider.GetComponent<BarrelInfo>() == null) {
                float dist = Vector3.Distance(obj.transform.position, hit.point);
                obj.transform.position -= Vector3.up * dist - new Vector3(0, 0.5f, 0);
            }
            //if its a barrel, snaps position to on top of the barrel
            else if (hit.collider.GetComponent<BarrelInfo>() != null && !isThrowing) {
                obj.transform.position = hit.collider.transform.position + new Vector3(0, 1.0f, 0);

                //links head/tail chain components of barrelInfo on place
                if (obj.GetComponent<BarrelInfo>() != null) {
                    obj.GetComponent<BarrelInfo>().tail = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<BarrelInfo>().head = obj.gameObject;
                }
            }
        }
    }

}