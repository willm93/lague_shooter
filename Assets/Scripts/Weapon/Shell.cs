using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof(Rigidbody))]
public class Shell : MonoBehaviour
{
    Rigidbody myRigidbody;
    public Vector2 forceMinMax;
    public float lifeTime = 1f;
    public Vector3 initDirection;

    void Start()
    {
        myRigidbody = this.GetComponent<Rigidbody>();
        float force = Random.Range(forceMinMax.x, forceMinMax.y);
        myRigidbody.AddForce(initDirection * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);
        Destroy(this.gameObject, lifeTime);
    }
}
