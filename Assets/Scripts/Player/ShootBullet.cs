using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootBullet : MonoBehaviour
{
    PlayerInput playerInput;

    public GameObject bullet;

    // Bullet force
    public float shootForce = 150f;

    //public float shootingDelay;

    // Point from where it shoots
    public Transform muzzlePoint;

    // Reference to aiming script
    private MouseAim mouseAim;

    private void Awake()
    {
        playerInput = new PlayerInput();

        // Key Down
        playerInput.ShootingControls.Shoot.started += OnShoot;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton()) // If Shoot button has been pressed
            Shoot();
    }

    // Start is called before the first frame update
    void Start()
    {
        mouseAim = GetComponent<MouseAim>();
    }

    private void Shoot()
    {
        Vector3 targetPos;

        Ray ray = new Ray(muzzlePoint.position, mouseAim.direction);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.collider.tag == "Enemy")
            {
                targetPos = raycastHit.point;
                Debug.Log("HIIIIIT");
            }
            else
                targetPos = ray.GetPoint(75); // Just a point far away from the player (missed)
        }
        else
            targetPos = ray.GetPoint(75); // Just a point far away from the player (missed)
        
        Vector3 direction = targetPos - muzzlePoint.position;

        // Instantiate bullet in gun point
        GameObject currentBullet = Instantiate(bullet, muzzlePoint.position, Quaternion.identity);
        
        // Rotate bullet to shoot direction
        currentBullet.transform.forward = direction.normalized;

        // Impulses the bullet to shoot direction
        currentBullet.GetComponent<Rigidbody>().AddForce(direction.normalized * shootForce, ForceMode.Impulse);
    }

    private void OnEnable()
    {
        playerInput.ShootingControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.ShootingControls.Disable();
    }
}
