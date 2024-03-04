using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 100f;
    public int damage = 5;
    float lifetime = 2f;
    float movingTargetCorrection = 0.1f;

    float moveDistance;
    Ray distanceRay;
    RaycastHit hitInfo;
    public LayerMask collisionMask;


    void Start()
    {
        Destroy(this.gameObject, lifetime);

        //if spawning inside a collider
        Collider[] initialColliders = Physics.OverlapSphere(this.transform.position, 0.1f, collisionMask);
        if (initialColliders.Length > 0){
            OnHitObject(initialColliders[0], this.transform.position);
        }
    }
    
    void Update()
    {
        moveDistance = speed * Time.deltaTime;
        DetectCollision(moveDistance);
        this.transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void DetectCollision(float distance)
    {
        distanceRay.origin = transform.position;
        distanceRay.direction = transform.forward;
        if (Physics.Raycast(distanceRay, out hitInfo, distance + movingTargetCorrection, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hitInfo.collider, hitInfo.point);
        }
    }

    void OnHitObject(Collider collider, Vector3 hitPoint){
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        damageableObject?.TakeHit(damage, hitPoint, transform.forward);
        Destroy(this.gameObject);
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }
}
