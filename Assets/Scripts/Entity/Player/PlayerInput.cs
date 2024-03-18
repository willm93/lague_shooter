using System;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent ( typeof (Player))]
[RequireComponent ( typeof (GunController))]
public class PlayerInput : MonoBehaviour
{
    public InputController input {get; private set;}
    Player player;
    GunController gunController;
    Camera gameCamera;

    public Transform crosshair;
    Vector3 movementDirection;
    Plane groundPlane;
    Vector3 mousePoint;
    Vector3 gunOffsetFromPlayer;

    bool sprintAttempted = false;
    public bool isPaused = false;
    public event Action OnPause;
    public event Action OnResume;

    void Awake()
    {
        input = new InputController();
        player = GetComponent<Player>();
        gunController = GetComponent<GunController>();
        gameCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunPosition().y);
    }

    void Update()
    {
        gunOffsetFromPlayer = gunController.GunPosition() - transform.position;
        crosshair.position = GetMousePoint() + gunOffsetFromPlayer;
        player.SetDirection(GetMousePoint() - transform.position);
        player.SetVelocity(movementDirection, sprintAttempted);
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

        input.UI.Pause.performed += OnPausePerformed;
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

        input.Player.Reload.performed -= OnReloadPerformed;

        input.UI.Pause.performed -= OnPausePerformed;
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

    void OnPausePerformed(InputAction.CallbackContext value)
    {
        isPaused = !isPaused;
        if(isPaused)
        {
            input.Player.Disable();
            Time.timeScale = 0;
            OnPause?.Invoke();
        } else 
        {
            input.Player.Enable();
            Time.timeScale = 1;
            OnResume?.Invoke();
        }
    }

    public void ExternalUnpause() //from UI
    {
        isPaused = false;
        input.Player.Enable();
        Time.timeScale = 1;
        OnResume?.Invoke();
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
