using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 10f;
    public Vector3 offset;
    public Vector2 cameraLimitWidth;
    public Vector2 cameraLimitHeight;

    Transform target;

    void LateUpdate()
    {
        GameObject targetGO = GameObject.FindGameObjectWithTag("MainPlayer");


        if (targetGO)
        {
            target = targetGO.transform;

            Vector3 desiredPosition = target.position + offset;

            desiredPosition.x = Mathf.Clamp(desiredPosition.x, cameraLimitWidth.x, cameraLimitWidth.y);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, cameraLimitHeight.x, cameraLimitHeight.y);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}