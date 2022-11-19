using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunProjectile : MonoBehaviour
{
    PlayerInput playerInput;

    public GameObject bullet;

    // Bullet forces
    public float shootForce, upwardForce;

    public float shootingDelay;

    private void Awake()
    {
        playerInput = new PlayerInput();

        // Key Down
        playerInput.ShootingControls.Shoot.started += OnShootingInput;
    }

    void OnShootingInput(InputAction.CallbackContext context)
    {
        //currentMovementInput = context.ReadValue<bool>();
        //currentMovement.x = currentMovementInput.x * speed;
        //currentMovement.z = currentMovementInput.y * speed;
        //isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
