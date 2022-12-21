using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 10f;
    public Vector3 offset;
    public Vector2 cameraLimitWidth;
    public Vector2 cameraLimitHeight;

    Transform target;
    bool isTargetActive = false;

    private void Start()
    {
        isTargetActive = false;
    }

    void LateUpdate()
    {
        if (!isTargetActive)
        {
            // TODO Instance Players without Player Tag
            //target = GameObject.Find(NetworkingManager.Instance.networking.myUserData.networkID).transform;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            isTargetActive = true;
        }

        Vector3 desiredPosition = target.position + offset;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, cameraLimitWidth.x, cameraLimitWidth.y);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, cameraLimitHeight.x, cameraLimitHeight.y);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}