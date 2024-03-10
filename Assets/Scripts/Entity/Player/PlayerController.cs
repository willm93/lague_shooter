using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
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

    public float maxStamina = 100f;
    public float stamina {get; private set;}
    public float staminaUsageRate = 10f;
    public float staminaRechargeRate = 5f;
    public float staminaRechargeCooldown = 2f;

    bool staminaOnCooldown;
    bool isSprinting;
    
    void Start()
    {
        myRigidbody = this.GetComponent<Rigidbody>();
        stamina = maxStamina;
        initRotationTime = rotationTime;
    }
    
    void FixedUpdate()
    {
        myRigidbody.Move(myRigidbody.position + (velocity * Time.fixedDeltaTime), Quaternion.LookRotation(direction, Vector3.up));
    }

    public void SetVelocity(Vector3 direction, bool sprintAttempted)
    {
        if (sprintAttempted && stamina > 0 && !staminaOnCooldown){
            isSprinting = true;
            stamina -= staminaUsageRate * Time.deltaTime;
            velocity = direction * sprintSpeed;
        } else {
            isSprinting = false;
            velocity = direction * moveSpeed;
        }

        if (!isSprinting && stamina < maxStamina){
            stamina += staminaRechargeRate * Time.deltaTime;
        }

        if (stamina <= 0 && !staminaOnCooldown){
            staminaOnCooldown = true;
            StartCoroutine(StaminaCooldown());
        }            
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

    IEnumerator StaminaCooldown()
    {
        yield return new WaitForSeconds(staminaRechargeCooldown);
        staminaOnCooldown = false;
    }
}
