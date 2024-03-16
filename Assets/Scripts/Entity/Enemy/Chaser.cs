using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent ( typeof (NavMeshAgent))]
public class Chaser : Enemy
{
    public override string Name {get => "Chaser";}
    public enum State {Idle, Chasing, Attacking};
    [SerializeField] State currentState;
    IEnumerator currentRoutine;
    GameObject spawnTile;

    LivingEntity targetEntity;
    Transform targetTransform;
    NavMeshAgent pathfinder;
    [SerializeField] float pathfinderSpeed;
    [SerializeField] float pathfinderAccel;
    [SerializeField] float pathfinderAngularSpeed;
    
    [SerializeField] int attackDamage = 5;
    [SerializeField] Color attackColor;
    [SerializeField] float attackDistance = 0.5f;
    [SerializeField] float timeBetweenAttacks = 2f;
    float nextAttackTime;

    float myCollisionRadius;
    float targetCollisionRadius;
    bool hasNavMesh;

    protected override void Awake()
    {
        base.Awake();
        pathfinder = GetComponent<NavMeshAgent>();
        mapGen = FindAnyObjectByType<MapGenerator>();
    }

    public override void SetCharacteristics(EnemyWave wave, GameObject _spawnTile)
    {
        pathfinderSpeed = wave.enemySpeed;
        maxHealth = wave.enemyHealth;
        attackDamage = wave.attackDamage;
        myMaterial.color = wave.enemyColor;
        attackColor = wave.attackColor;
        spawnTile = _spawnTile;
    }

    protected override void Start()
    {
        base.Start();
        pathfinder.speed = pathfinderSpeed;
        pathfinder.acceleration = pathfinderAccel;
        pathfinder.angularSpeed = pathfinderAngularSpeed;
        originalColor = myMaterial.color;

        targetEntity = GameObject.FindGameObjectWithTag("Player")?.GetComponent<LivingEntity>();
        hasNavMesh = FindAnyObjectByType<NavMeshSurface>() != null;
        if(targetEntity != null && hasNavMesh){
            targetTransform = targetEntity.transform;
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = targetTransform.GetComponent<CapsuleCollider>().radius;
            pathfinder.stoppingDistance = myCollisionRadius + targetCollisionRadius + attackDistance / 2;
            
            currentRoutine = Chase();
            StartCoroutine(currentRoutine);
        } else {
            currentState = State.Idle;
        }

        mapGen.UnclaimTile(spawnTile);
    }

    void Update()
    {
        if (targetEntity != null && Time.time > nextAttackTime && hasNavMesh) {
            if (TargetInRange()){

                if (currentRoutine != null){
                    StopCoroutine(currentRoutine);
                }
                currentRoutine = Attack();
                StartCoroutine(currentRoutine);
                nextAttackTime = Time.time + timeBetweenAttacks;
            }
        }
        if (targetEntity != null && currentState == State.Idle && hasNavMesh){
            currentRoutine = Chase();
            StartCoroutine(currentRoutine);
        }
    }

    bool TargetInRange()
    {
        float sqrDistanceToTarget = (targetTransform.position - transform.position).sqrMagnitude;
        return sqrDistanceToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2);
    }

    void OnTargetDeath()
    {
        currentState = State.Idle;
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        Vector3 targetPosition = targetTransform.position - (directionToTarget * targetCollisionRadius);

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
                targetEntity.TakeDamage(attackDamage);
            }
            
            //animate
            percent += Time.deltaTime * lungeSpeed;
            interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; //quadratic function where f(percent) <= 1
            transform.position = Vector3.Lerp(originalPosition, targetPosition, interpolation);

            yield return null;
        }
        myMaterial.color = originalColor;

        currentState = State.Idle;
        currentRoutine = null;
    }

    IEnumerator Chase()
    {
        currentState = State.Chasing;

        float refreshRate = 0.1f;
        while (targetEntity != null){
            pathfinder.SetDestination(targetEntity.transform.position);
            yield return new WaitForSeconds(refreshRate);
        }

        currentState = State.Idle;
        currentRoutine = null;
    }

}
