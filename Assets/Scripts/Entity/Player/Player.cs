using System.Collections;
using UnityEngine;

[RequireComponent ( typeof (Rigidbody))]
[RequireComponent ( typeof (PlayerInput))]
[RequireComponent ( typeof (GunController))]
[RequireComponent ( typeof (PowerupController))]
public class Player : LivingEntity
{
    Vector3 velocity;
    Vector3 direction;
    Vector3 directionVelocity;
    [SerializeField] float rotationTime = 0.01f;
    [SerializeField] float limitedRotationTime = 0.45f;
    float initRotationTime;
    public bool bigRecoil;

    Rigidbody myRigidbody;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    float currentSpeed;

    public float maxStamina = 100f;
    public float stamina {get; private set;}
    public float staminaUsageRate = 10f;
    public float staminaRechargeRate = 5f;
    public float staminaRechargeCooldown = 2f;

    bool staminaOnCooldown;
    bool isSprinting;
    bool infiniteStamina = false;
    
    protected override void Start()
    {
        base.Start();
        myRigidbody = this.GetComponent<Rigidbody>();
        stamina = maxStamina;
        initRotationTime = rotationTime;
        Cursor.visible = false;
    }
    
    void FixedUpdate()
    {
        myRigidbody.Move(myRigidbody.position + (velocity * Time.fixedDeltaTime), Quaternion.LookRotation(direction, Vector3.up));
    }

    public void SetVelocity(Vector3 direction, bool sprintAttempted)
    {
        isSprinting = CanSprint() && sprintAttempted && direction != Vector3.zero;
        UpdateStamina();
        currentSpeed =  isSprinting ? sprintSpeed : moveSpeed;

        velocity = direction * currentSpeed;          
    }

    bool CanSprint()
    {
        return infiniteStamina || (stamina > 0 && !staminaOnCooldown);
    }

    void UpdateStamina()
    {
        if (isSprinting && !infiniteStamina)
        {
            stamina -= staminaUsageRate * Time.deltaTime;
        }

        if ((!isSprinting || infiniteStamina) && stamina < maxStamina)
        {
            stamina += staminaRechargeRate * Time.deltaTime;
        }

        if (stamina <= 0 && !staminaOnCooldown)
        {
            staminaOnCooldown = true;
            StartCoroutine(StaminaCooldown());
        }
    }

    IEnumerator StaminaCooldown()
    {
        yield return new WaitForSeconds(staminaRechargeCooldown);
        staminaOnCooldown = false;
    }

    public void SetDirection(Vector3 _direction)
    {
        _direction.y = 0;
        direction = Vector3.SmoothDamp(transform.forward, _direction, ref directionVelocity, rotationTime);
    }

    public void LimitRotation(bool isOn)
    {
        if (isOn) {
            rotationTime = limitedRotationTime;
        } else {
            rotationTime = initRotationTime;
        }
    }
    
    public void BigRecoil()
    {
        myRigidbody.AddForce(-transform.forward * 4250f);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void InfiniteStamina(bool isOn)
    {
        infiniteStamina = isOn;
    }
}
