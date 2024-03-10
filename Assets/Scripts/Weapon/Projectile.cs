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

    public int penetrationCount;
    int currentPenCount = 0;
    Collider previousCollider;


    void Start()
    {
        Destroy(this.gameObject, lifetime);

        //if spawning inside a collider
        Collider[] initialColliders = Physics.OverlapSphere(this.transform.position, 0.1f, collisionMask);
        if (initialColliders.Length > 0){
            OnHitObject(initialColliders[0]);
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
            OnHitObject(hitInfo.collider);
        }
    }

    void OnHitObject(Collider collider){
        if (collider != previousCollider){
            IDamageable damageableObject = collider.GetComponent<IDamageable>();
            damageableObject?.TakeHit(damage, transform.forward);
            previousCollider = collider;

            if(collider.CompareTag("Enemy")){
                if (currentPenCount == penetrationCount){
                    Destroy(this.gameObject);    
                }
                currentPenCount++;
            } else {
                Destroy(this.gameObject);
            }
        }
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }
}
