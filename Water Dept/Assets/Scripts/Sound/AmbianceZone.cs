using Unity.Cinemachine;
using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    [Tooltip("Cinemachine Path to follow")]
    [SerializeField] CinemachineSplineCart _cart;

    [Tooltip("How far to offset the sfx infront of the camera")]
    [SerializeField] float offset = 8f;

    //This being deprecated is stupid. The Spline system hasn't got a closes position setting? WHY!?@?@ 
    ///private CinemachinePathBase.PositionUnits m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;
    //nvm lol, they made it easier

    private void Start()
    {
        if(_cart == null)
            _cart = GetComponent< CinemachineSplineCart>();

        if(_cart.TrackingTarget == null)
            _cart.TrackingTarget = Camera.main.transform;
    }

    private void LateUpdate()
    {
        Vector3 Sub = transform.position - _cart.TrackingTarget.position;
        Vector3 Spline = transform.right;

        // Attach object to player on enter
        if (Vector3.Dot(Sub, Spline) > 0)
        {
            transform.position = Vector3.Lerp(transform.position, _cart.TrackingTarget.position + _cart.TrackingTarget.forward * offset, 0.2f);
            transform.rotation = _cart.TrackingTarget.rotation;
        }
    }
}
