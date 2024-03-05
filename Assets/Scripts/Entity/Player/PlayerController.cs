using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Vector3 direction;
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
    }
    
    void FixedUpdate()
    {
        myRigidbody.Move(myRigidbody.position + (velocity * Time.deltaTime), Quaternion.LookRotation(direction, Vector3.up));
    }

    public void Move(Vector3 direction, bool sprintAttempted)
    {
        if (sprintAttempted && stamina > 0 && !staminaOnCooldown){
            isSprinting = true;
            stamina -= staminaUsageRate * Time.fixedDeltaTime;
            velocity = direction * sprintSpeed;
        } else {
            isSprinting = false;
            velocity = direction * moveSpeed;
        }

        if (!isSprinting && stamina < maxStamina){
            stamina += staminaRechargeRate * Time.fixedDeltaTime;
        }

        if (stamina <= 0 && !staminaOnCooldown){
            staminaOnCooldown = true;
            StartCoroutine(StaminaCooldown());
        }            
    }

    public void Face(Vector3 _direction)
    {
        _direction.y = 0;
        direction = _direction;
    }

    IEnumerator StaminaCooldown()
    {
        yield return new WaitForSeconds(staminaRechargeCooldown);
        staminaOnCooldown = false;
    }
}
