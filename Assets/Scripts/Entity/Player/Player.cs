using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent ( typeof (PlayerController))]
[RequireComponent ( typeof (GunController))]
public class Player : LivingEntity
{
    InputController input;
    PlayerController controller;
    GunController gunController;
    Camera gameCamera;

    public Transform crosshair;
    Vector3 movementDirection;
    Plane groundPlane;
    Vector3 mousePoint;
    Vector3 gunOffsetFromPlayer;

    bool sprintAttempted = false;

    void Awake()
    {
        Cursor.visible = false;
        input = new InputController();
        controller = this.GetComponent<PlayerController>();
        gunController = this.GetComponent<GunController>();
        gameCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunPosition().y);
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null){
            spawner.OnNewWave += OnNewWave;
        }
    }

    void Update()
    {
        gunOffsetFromPlayer = gunController.GunPosition() - this.transform.position;
        crosshair.position = GetMousePoint() + gunOffsetFromPlayer;
        controller.Face(GetMousePoint() - this.transform.position);
        controller.Move(movementDirection, sprintAttempted);
    }

    void OnNewWave(int _i)
    {
        gunController.NextGun();
    }

    void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCanceled;

        input.Player.Sprint.performed += OnSprintPerformed;
        input.Player.Sprint.canceled += OnSprintCanceled;

        input.Player.Fire.performed += OnFirePerformed;
        input.Player.Fire.canceled += OnFireCanceled;

        input.Player.NextGun.performed += OnNextGunPerformed;

        input.Player.Reload.performed += OnReloadPerformed;
    }

    void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCanceled;

        input.Player.Sprint.performed -= OnSprintPerformed;
        input.Player.Sprint.canceled -= OnSprintCanceled;

        input.Player.Fire.performed -= OnFirePerformed;
        input.Player.Fire.canceled -= OnFireCanceled;

        input.Player.NextGun.performed -= OnNextGunPerformed;

        input.Player.Reload.performed += OnReloadPerformed;
    }

    void OnMovementPerformed(InputAction.CallbackContext value)
    {
        movementDirection = value.ReadValue<Vector3>().normalized;
    }

    void OnMovementCanceled(InputAction.CallbackContext value)
    {
        movementDirection = Vector3.zero;
    }

    void OnSprintPerformed(InputAction.CallbackContext value)
    {
        sprintAttempted = true;
    }

    void OnSprintCanceled(InputAction.CallbackContext value)
    {
        sprintAttempted = false;
    }

    void OnFirePerformed(InputAction.CallbackContext value)
    {
        gunController.OnTriggerHold();
    }

    void OnFireCanceled(InputAction.CallbackContext value)
    {
        gunController.OnTriggerRelease();
    }

    void OnNextGunPerformed(InputAction.CallbackContext value)
    {
        gunController.NextGun();
    }

    void OnReloadPerformed(InputAction.CallbackContext value)
    {
        gunController.Reload();
    }

    Vector3 GetMousePoint()
    {
        Ray cameraToMouseRay = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (groundPlane.Raycast(cameraToMouseRay, out float rayDistance))
        {
            mousePoint = cameraToMouseRay.GetPoint(rayDistance);
        }
        return mousePoint;
    }
}
