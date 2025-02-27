using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public static FreeCameraController Singleton
    {
        get => mSingleton;
        set
        {
            if (value == null)
                mSingleton = null;
            else if (mSingleton == null)
                mSingleton = value;
            else if (mSingleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(FreeCameraController)}!");
            }
        }
    }
    private static FreeCameraController mSingleton;

    private Transform target;

    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    private void LateUpdate()
    {
        if (target != null)
            transform.SetPositionAndRotation(target.position, target.rotation);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

#region JAMES_CINEMACHINE WORK
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class FreeCameraController : MonoBehaviour
{
    public static FreeCameraController Singleton
    {
        get => mSingleton;
        set
        {
            if (value == null)
                mSingleton = null;
            else if (mSingleton == null)
                mSingleton = value;
            else if (mSingleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(FreeCameraController)}!");
            }
        }
    }
    private static FreeCameraController mSingleton;

    private CinemachineCamera target;

    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    *//*private void LateUpdate()
    {
        if (target != null)
            transform.SetPositionAndRotation(target.position, target.rotation);
    }
    *//*

    //gets calledon local spawn
    public void SetTarget(CinemachineCamera newTarget)
    {
        target = newTarget;
        target.Target.TrackingTarget = transform;
    }
}*/
#endregion