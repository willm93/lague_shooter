using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent ( typeof (NavMeshAgent))]
public class Justicar : Enemy
{
    public override string Name {get => "Justicar";}
    public override bool NeededForCount {get => true;}

    public enum State {Idle, Chasing, Attacking};
    [SerializeField] State currentState;
    IEnumerator currentRoutine;
    GameObject spawnTile;

    [SerializeField] Transform weaponHoldPoint;
    [SerializeField] GameObject weapon;
    [SerializeField] LayerMask obstacleMask;

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

        if(targetEntity != null && hasNavMesh)
        {
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
        if (targetEntity != null && Time.time > nextAttackTime && TargetInRange() && hasNavMesh) 
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = Attack();
            StartCoroutine(currentRoutine);
            nextAttackTime = Time.time + timeBetweenAttacks;
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

        float lungeDistance = 12f;
        float chargeTime = 1f;
        float lungeSpeed = 1f;
        float percent = 0;

        //charge
        while (percent <= 1)
        {
            percent += Time.deltaTime / chargeTime;

            transform.rotation = Quaternion.LookRotation(targetTransform.position - transform.position, Vector3.up);
            weaponHoldPoint.transform.Rotate(Vector3.right * 90 * Time.deltaTime / chargeTime);
            yield return null;
        }
        percent = 0;

        //lunge
        myMaterial.color = attackColor;
        //AudioManager.instance.PlaySound(attackSound);
        Vector3 targetPosition = FindTargetPosition(lungeDistance);
        yield return new WaitForSeconds(0.2f); //to give a chance to dodge
        while (percent <= 1)
        {    
            //animate
            percent += Time.deltaTime * lungeSpeed;
            transform.position = Vector3.Lerp(transform.position, targetPosition, percent);
            yield return null;
        }
        weaponHoldPoint.transform.Rotate(Vector3.right * -90);
        myMaterial.color = originalColor;

        currentState = State.Idle;
        currentRoutine = null;
    }

    IEnumerator Chase()
    {
        currentState = State.Chasing;

        float refreshRate = 0.05f;
        while (targetEntity != null)
        {
            pathfinder.SetDestination(targetEntity.transform.position);
            yield return new WaitForSeconds(refreshRate);
        }

        currentState = State.Idle;
        currentRoutine = null;
    }

    Vector3 FindTargetPosition(float lungeDistance)
    {
        Vector3 targetPosition;   
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        directionToTarget.y = 0; //ignore height difference

        Ray ray = new Ray(transform.position, directionToTarget);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, lungeDistance, obstacleMask))
        {
            targetPosition = hitInfo.point;
        } else
        {
            targetPosition = transform.position + directionToTarget * lungeDistance;
        }

        return targetPosition;
    }
}
