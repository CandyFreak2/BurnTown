using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class DecalRandomizer : MonoBehaviour
{
    [SerializeField] DecalProjector projector;
    [SerializeField] List<Material> decals = new List<Material>();

    private void Awake()
    {
        projector = GetComponent<DecalProjector>();
        RandomizeDecal();
    }

    public void RandomizeDecal()
    {
        transform.forward = transform.parent.forward;
        transform.Rotate(Vector3.forward, Random.Range(0, 360), Space.Self);

        if(decals.Count > 0) 
            projector.material = decals[Random.Range(0, decals.Count)];
    }
}
