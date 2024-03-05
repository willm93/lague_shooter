using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent ( typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking};
    public State currentState;
    public ParticleSystem deathEffectPrefab;
    NavMeshAgent pathfinder;
    public float pathfinderSpeed;
    public float pathfinderAccel;
    public float pathfinderAngularSpeed;
    LivingEntity targetEntity;
    Transform target;
    public AudioClip attackSound;
    public AudioClip deathSound;
    

    public int attackDamage = 5;
    public Color attackColor;
    public float attackDistance = 0.5f;
    public float timeBetweenAttacks = 2f;
    float nextAttackTime;
    float sqrDistanceToTarget;
    bool hasTarget;
    bool deathEffectPlayed = false;

    float myCollisionRadius;
    float targetCollisionRadius;

    Material myMaterial;
    Color originalColor;

    void Awake()
    {
        pathfinder = this.GetComponent<NavMeshAgent>();
        myMaterial = this.GetComponent<Renderer>().material;
    }

    protected override void Start()
    {
        base.Start();
        pathfinder.speed = pathfinderSpeed;
        pathfinder.acceleration = pathfinderAccel;
        pathfinder.angularSpeed = pathfinderAngularSpeed;
        originalColor = myMaterial.color;

        targetEntity = GameObject.FindGameObjectWithTag("Player")?.GetComponent<LivingEntity>();
        if(targetEntity != null){
            target = targetEntity.transform;
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = this.GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            pathfinder.stoppingDistance = myCollisionRadius + targetCollisionRadius + attackDistance / 2;

            currentState = State.Chasing;
            hasTarget = true;
            StartCoroutine(MoveToTarget());
        } else {
            currentState = State.Idle;
            hasTarget = false;
        }
    }

    public void SetCharacteristics(float moveSpeed, int health, int damage, Color color, Color _attackColor)
    {
        pathfinderSpeed = moveSpeed;
        maxHealth = health;
        attackDamage = damage;
        myMaterial.color = color;
        attackColor = _attackColor;
    }

    void Update()
    {
        //attack if player in range
        if (hasTarget && Time.time > nextAttackTime) {
            sqrDistanceToTarget = (target.position - this.transform.position).sqrMagnitude;
            if (sqrDistanceToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2)){
                StartCoroutine(Attack());
                nextAttackTime = Time.time + timeBetweenAttacks;
            }
        }

        //correct state
        if (!hasTarget) {
            currentState = State.Idle;
        }
    }

    public override void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (damage >= currentHealth && !deathEffectPlayed){            
            float effectLifetime = deathEffectPrefab.main.startLifetime.constant;
            ParticleSystem deathEffect = Instantiate<ParticleSystem>(deathEffectPrefab, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            deathEffect.GetComponent<Renderer>().material.color = originalColor;
            deathEffect.Play();
            Destroy(deathEffect.gameObject, effectLifetime);
            deathEffectPlayed = true;
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    protected override void Die()
    {
        if (deathSound != null){
            AudioManager.instance.PlaySound(deathSound);
        } else {
            AudioManager.instance.PlaySound("Enemy Death");
        }
        
        base.Die();
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = this.transform.position;
        Vector3 directionToTarget = (target.position - this.transform.position).normalized;
        Vector3 targetPosition = target.position - (directionToTarget * targetCollisionRadius);

        float lungeSpeed = 3f;
        float percent = 0;
        float interpolation;
        bool hasAppliedDamage = false;

        myMaterial.color = attackColor;
        AudioManager.instance.PlaySound(attackSound);
        while (percent <= 1){
            //apply damage halfway through animation
            if (percent > 0.5f && !hasAppliedDamage){
                hasAppliedDamage = true;
                targetEntity.TakeDamage(5);
            }
            
            //animate
            percent += Time.deltaTime * lungeSpeed;
            interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; //quadratic function where f(percent) <= 1
            this.transform.position = Vector3.Lerp(originalPosition, targetPosition, interpolation);

            yield return null;
        }
        myMaterial.color = originalColor;
        
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator MoveToTarget()
    {
        float refreshRate = 0.1f;
        //Vector3 directionToTarget;

        while (hasTarget){
            if (currentState == State.Chasing) {
                //directionToTarget = (target.position - this.transform.position).normalized;
                //correctedTargetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistance/2);
                pathfinder.SetDestination(targetEntity.transform.position);
            }
            yield return new WaitForSeconds(refreshRate);
        }
        currentState = State.Idle;
    }

}
