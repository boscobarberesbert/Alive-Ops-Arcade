using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimPlaneController : MonoBehaviour
{
    Transform cameraTransform;
    Vector3 cameraOffset;
    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        cameraOffset = Camera.main.GetComponent<CameraFollow>().offset;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraTransform.position - cameraOffset;
    }
}
