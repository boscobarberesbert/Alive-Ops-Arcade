using UnityEngine;

public class AimPlaneController : MonoBehaviour
{
    Transform cameraTransform;
    Vector3 cameraOffset;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        cameraOffset = Camera.main.GetComponent<CameraFollow>().offset;
    }

    void Update()
    {
        transform.position = cameraTransform.position - cameraOffset;
    }
}