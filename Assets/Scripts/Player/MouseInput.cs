using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInput : MonoBehaviour
{
    // Aim members
    public float rotationSpeed = 5f;
    public Transform gunPoint;

    Vector3 worldPosition;
    Vector3 aimDirection;
    Quaternion lookRotation;

    public bool isAiming = false;

    [SerializeField] LayerMask mouseColliderLayerMask;

    PlayerInput playerInput;

    void Awake()
    {
        playerInput = new PlayerInput();

        // Key Down
        playerInput.ShootingControls.Shoot.started += OnShoot;
    }

    void OnShoot(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton()) // If Shoot button has been pressed
        {
            NetworkingManager.Instance.OnShoot();
            GetComponent<Shooting>().Shoot(aimDirection);
        }
    }

    void Update()
    {
        isAiming = Mouse.current.delta.ReadValue() != Vector2.zero;

        HandleAim();
    }

    void HandleAim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Camera.main.farClipPlane, mouseColliderLayerMask))
        {
            worldPosition = raycastHit.point;
        }

        // Find vector pointing from the player position to the mouse world position
        aimDirection = (worldPosition - gunPoint.position).normalized;

        // We want to only aim in the xz plane
        aimDirection.y = 0;

        // Perform rotation to look
        lookRotation = Quaternion.LookRotation(aimDirection);

        // Rotate over time according to speed
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    void OnEnable()
    {
        playerInput.ShootingControls.Enable();
    }

    void OnDisable()
    {
        playerInput.ShootingControls.Disable();
    }
}