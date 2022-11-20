using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    public float gravity = -9.8f;

    PlayerInput playerInput;
    CharacterController characterController;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;

    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();

        // Key Down
        playerInput.CharacterControls.Move.started += OnMovementInput;

        // Key Up
        playerInput.CharacterControls.Move.canceled += OnMovementInput;

        // Performed: continues to update changes
        playerInput.CharacterControls.Move.performed += OnMovementInput;
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * speed;
        currentMovement.z = currentMovementInput.y * speed;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    private void HandleGravity()
    {
        if(characterController.isGrounded)
        {
            // Constant small force to keep the character controller grounded
            // as it considers itself floating if there is 0 downward movement
            float groundedGravity = -0.05f;
            currentMovement.y = groundedGravity;
        }
        else
        {
            currentMovement.y += gravity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleGravity();

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
