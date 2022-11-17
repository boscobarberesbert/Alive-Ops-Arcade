using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector2 cameraLimit;

    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, -cameraLimit.x, cameraLimit.x);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, -cameraLimit.y, cameraLimit.y);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}