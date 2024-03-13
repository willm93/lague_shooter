using System.Collections;
using UnityEngine;

public class Pounder : Enemy
{
    public enum State {Idle, Moving, Pounding};
    [SerializeField] State currentState;
    IEnumerator currentRoutine;
    bool moved;
    bool pounded;

    Vector3 velocity;
    [SerializeField] float eta = 1f;

    LivingEntity targetEntity;
    MapGenerator mapGen;
    
    [SerializeField] int attackDamage = 20;
    [SerializeField] float pounderWaveSpeed = 1f;
    [SerializeField] Color attackColor;
    [SerializeField] PounderWave pounderWavePrefab;

    protected override void Start()
    {
        base.Start();
        mapGen = FindAnyObjectByType<MapGenerator>();
        targetEntity = FindAnyObjectByType<Player>()?.GetComponent<LivingEntity>();
        if(targetEntity != null){
            targetEntity.OnDeath += OnTargetDeath;

            currentRoutine = Pound();
            StartCoroutine(Pound());
        } else {
            currentState = State.Idle;
        }
    }

    public override void SetCharacteristics(EnemyWave wave)
    {
        maxHealth = wave.enemyHealth;
        attackDamage = wave.attackDamage;
        myMaterial.color = wave.enemyColor;
        attackColor = wave.attackColor;
    }

    void Update()
    {
        if (targetEntity != null && currentState == State.Idle && currentRoutine == null && pounded){
            pounded = false;
            currentRoutine = Move();
            StartCoroutine(Move());
        }
        if (targetEntity != null && currentState == State.Idle && currentRoutine == null && moved){
            moved = false;
            currentRoutine = Pound();
            StartCoroutine(Pound());
        }
    }

    void OnTargetDeath()
    {
        currentState = State.Idle;
    }

    IEnumerator Pound()
    {
        currentState = State.Pounding;

        Vector3 upPosition = transform.position + Vector3.up;
        Vector3 downPosition = transform.position;
        Vector3 direction;
        PounderWave newpounderWave;

        while (Vector3.SqrMagnitude(upPosition - transform.position) > 0.01f){
            direction = (upPosition - transform.position).normalized;
            transform.Translate(1f * Time.deltaTime * direction);
            yield return null;
        }
        
        myMaterial.color = attackColor;
        //AudioManager.instance.PlaySound(attackSound);
        
        while (Vector3.SqrMagnitude(downPosition - transform.position) > 0.01f){
            direction = (downPosition - transform.position).normalized;
            transform.Translate(4f * Time.deltaTime * direction);
            yield return null;
        }

        for(int i = 0; i < 4; i++){
            newpounderWave = Instantiate(pounderWavePrefab, transform.position + Vector3.up, Quaternion.Euler(Vector3.up * 90 * i));
            newpounderWave.damage = attackDamage;
            newpounderWave.speed = pounderWaveSpeed;
        }

        myMaterial.color = originalColor;
        yield return new WaitForSeconds(2f);

        currentState = State.Idle;
        currentRoutine = null;
        pounded = true;
    }

    IEnumerator Move()
    {
        currentState = State.Moving;        

        GameObject targetTile = mapGen.GetRandomAdjacentTile(transform.position);
        Vector3 targetPosition;

        if (targetTile != null){
            targetPosition = targetTile.transform.position;
            
            while (Vector3.SqrMagnitude(targetPosition - transform.position) > 0.01f){
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, eta);
                yield return null;
            }
        } else {
            yield return new WaitForSeconds(5f);
        }
        
        currentState = State.Idle;
        currentRoutine = null;
        moved = true;
    }

}

