using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -7.5f);
    public Vector3 firstPersonOffset = new Vector3(-0.4f, 0.75f, -0.35f);
    private Vector3 offsetWorld;

    public float followSpeed = 10.0f;
    public float lookSpeed = 10.0f;

    public bool isThirdPerson = true;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isThirdPerson = !isThirdPerson;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 offset = isThirdPerson ? thirdPersonOffset : firstPersonOffset;

        //Get offset in world space
        offsetWorld = target.TransformDirection(offset);

        //Move Camera
        Vector3 desiredPos = target.position + offsetWorld;
        //Interpolate to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.fixedDeltaTime);

        //Orient Camera
        if (isThirdPerson)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            //Interpolate camera movement
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lookSpeed * Time.fixedDeltaTime);
        }
        else
        {
            //If first person align camera with target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, lookSpeed * Time.fixedDeltaTime);
        }
    }

    //public Transform target;
    //public Vector3 offset = new Vector3(0, 7, -7.5f);
    //private Vector3 offsetWorld;

    //public float folllowSpeed = 10.0f;
    //public float lookSpeed = 10.0f;

    //// Update is called once per frame
    //void FixedUpdate()
    //{
    //    //Get offset in world space
    //    offsetWorld = target.localToWorldMatrix * offset;

    //    //Move Camera
    //    Vector3 desiredPos = target.position + offsetWorld;
    //    //Interpolate to desired position
    //    transform.position = Vector3.Lerp(transform.position, desiredPos, folllowSpeed * Time.fixedDeltaTime);

    //    //Orient Camera
    //    Vector3 directionToTarget = (target.position - transform.position).normalized;
    //    Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

    //    //Interpolate camera movement
    //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lookSpeed * Time.fixedDeltaTime);
    //}
}
