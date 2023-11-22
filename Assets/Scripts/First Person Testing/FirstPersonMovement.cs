using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonMovement : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    [SerializeField] private float moveSpeed;

    private float xRot;
    private float yRot;

    private FirstPersonInput input;
    private InputAction move;
    private InputAction look;
    private InputAction fire;

    private CharacterController controller;
    private FirstPersonPickup firstPersonPickup;

    private void Awake()
    {
        input = new FirstPersonInput();
        controller = GetComponent<CharacterController>();
        firstPersonPickup = GetComponent<FirstPersonPickup>();
    }

    private void OnEnable()
    {
        move = input.Player.Move;
        look = input.Player.Look;
        fire = input.Player.Fire;

        fire.performed += _ => Fire();

        move.Enable();
        look.Enable();
        fire.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        fire.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void Look()
    {
        Vector2 lookInput = look.ReadValue<Vector2>();
        yRot += lookInput.x * Time.deltaTime * sensitivity;
        xRot -= lookInput.y * Time.deltaTime * sensitivity;
        xRot = Mathf.Clamp(xRot, -75f, 75f);

        transform.rotation = Quaternion.Euler(xRot, yRot, 0);
    }

    private void Move()
    {
        Vector3 moveInput = move.ReadValue<Vector2>();
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.y = 0;
        controller.Move(moveDirection.normalized * Time.deltaTime * moveSpeed);
    }

    private void Fire()
    {
        firstPersonPickup.PickupRayHit();
    }
}
