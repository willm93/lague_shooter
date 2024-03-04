using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Vector3 direction;
    Rigidbody myRigidbody;
    
    void Start()
    {
        myRigidbody = this.GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
    {
        myRigidbody.Move(myRigidbody.position + (velocity * Time.deltaTime), Quaternion.LookRotation(direction));
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Face(Vector3 _direction)
    {
        _direction.y = 0;
        direction = _direction;
    }
}
