using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAim : MonoBehaviour
{
    // Aim members
    public float rotationSpeed = 5f;

    Vector3 worldPosition;
    Vector3 aimDirection;
    Quaternion lookRotation;

    public bool isAiming = false;

    [SerializeField] LayerMask mouseColliderLayerMask;

    // Shoot Members
    public GameObject bullet;
    public float shootForce = 150f;     // Bullet force
    public Transform muzzlePoint;       // Point from where it shoots

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
            Shoot();
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
        aimDirection = (worldPosition - transform.position).normalized;

        // We want to only aim in the xz plane
        aimDirection.y = 0;

        // Perform rotation to look
        lookRotation = Quaternion.LookRotation(aimDirection);

        // Rotate over time according to speed
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        Vector3 targetPos;

        Ray ray = new Ray(muzzlePoint.position, aimDirection);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            targetPos = raycastHit.point;
        }
        else
            targetPos = ray.GetPoint(75); // Just a point far away from the player (missed)

        Vector3 shootDirection = targetPos - muzzlePoint.position;

        // Instantiate bullet in gun point
        GameObject currentBullet = Instantiate(bullet, muzzlePoint.position, Quaternion.identity);

        // Set properties of the bullet we have shot
        currentBullet.GetComponent<Bullet>().Setup(shootDirection);

        // Impulses the bullet to shoot direction
        currentBullet.GetComponent<Rigidbody>().AddForce(shootDirection.normalized * shootForce, ForceMode.Impulse);
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