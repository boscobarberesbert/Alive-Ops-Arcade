using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAim : MonoBehaviour
{
    [SerializeField] private LayerMask mouseColliderLayerMask;

    public float rotationSpeed = 5f;

    private Vector3 screenPosition;
    private Vector3 worldPosition;

    [HideInInspector] public Vector3 direction;
    private Quaternion lookRotation;

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Camera.main.farClipPlane, mouseColliderLayerMask))
        {
            worldPosition = raycastHit.point;
        }

        // Find vector pointing from the player position to the mouse world position
        direction = (worldPosition - transform.position).normalized;
        
        // We want to only aim in the xz plane
        direction.y = 0;

        // Perform rotation to look
        lookRotation = Quaternion.LookRotation(direction);

        // Rotate over time according to speed
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }
}
