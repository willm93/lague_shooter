using System.Collections;
using UnityEngine;

public class Pounder : Enemy
{
    public override string Name {get => "Pounder";}
    public override bool NeededForCount {get => false;}
    GameObject currentTile;
    GameObject targetTile;

    public enum State {Idle, Moving, Pounding};
    [SerializeField] State currentState;
    IEnumerator currentRoutine;
    bool moved;
    bool pounded;

    Vector3 velocity;
    [SerializeField] float moveTime = 1f;
    LivingEntity targetEntity;
    [SerializeField] int healthMultiplier = 9;
    [SerializeField] int damageMultiplier = 3;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float pounderWaveSpeed = 1f;
    [SerializeField] Color attackColor;
    [SerializeField] PounderWave pounderWavePrefab;

    public override void SetCharacteristics(EnemyWave wave, GameObject spawnTile)
    {
        maxHealth = healthMultiplier * wave.enemyHealth;
        attackDamage = damageMultiplier * wave.attackDamage;
        myMaterial.color = wave.enemyColor;
        attackColor = wave.attackColor;
        currentTile = spawnTile;
        targetTile = spawnTile;

        transform.position = spawnTile.transform.position + Vector3.up * mapGen.TileScale * 0.5f * 0.95f;
        transform.localScale *= mapGen.TileScale * 0.95f;
    }
    
    protected override void Start()
    {
        base.Start();
        mapGen = FindAnyObjectByType<MapGenerator>();

        targetEntity = FindAnyObjectByType<Player>()?.GetComponent<LivingEntity>();
        if (targetEntity != null){
            targetEntity.OnDeath += OnTargetDeath;

            currentRoutine = Pound();
            StartCoroutine(Pound());
        } else {
            currentState = State.Idle;
        }
    }

    void Update()
    {
        if (targetEntity != null && currentState == State.Idle && currentRoutine == null && pounded)
        {
            pounded = false;
            currentRoutine = Move();
            StartCoroutine(Move());
        }
        if (targetEntity != null && currentState == State.Idle && currentRoutine == null && moved)
        {
            moved = false;
            currentRoutine = Pound();
            StartCoroutine(Pound());
        }
    }

    void OnTargetDeath()
    {
        currentState = State.Idle;
    }

    /*private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Enemy"))
            Debug.Log($"{gameObject.name} touching {other.gameObject.name}");       
    }*/

    IEnumerator Pound()
    {
        currentState = State.Pounding;

        Vector3 upPosition = transform.position + Vector3.up;
        Vector3 downPosition = transform.position;
        Vector3 direction;
        PounderWave newpounderWave;

        while (Vector3.SqrMagnitude(upPosition - transform.position) > 0.001f)
        {
            direction = (upPosition - transform.position).normalized;
            transform.Translate(1f * Time.deltaTime * direction);
            yield return null;
        }
        
        myMaterial.color = attackColor;
        //AudioManager.instance.PlaySound(attackSound);
        
        while (Vector3.SqrMagnitude(downPosition - transform.position) > 0.001f)
        {
            direction = (downPosition - transform.position).normalized;
            transform.Translate(5f * Time.deltaTime * direction);
            yield return null;
        }

        for(int i = 0; i < 4; i++)
        {
            Vector3 spawnPosition = new Vector3(
                transform.position.x + transform.localScale.x * Mathf.Sin(Mathf.PI * 0.5f * i) * 0.5f, 
                pounderWavePrefab.transform.localScale.y / 2, 
                transform.position.z + transform.localScale.z * Mathf.Cos(Mathf.PI * 0.5f * i) * 0.5f
            );
            newpounderWave = Instantiate(pounderWavePrefab, spawnPosition, Quaternion.Euler(Vector3.up * 90 * i));
            newpounderWave.transform.localScale = new Vector3(transform.localScale.x, newpounderWave.transform.localScale.y,newpounderWave.transform.localScale.z);
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

        GameObject newTargetTile = mapGen.ClaimRandomAdjacentOpenTile(transform.position);

        if (newTargetTile)
        {   
            targetTile = newTargetTile;     
            Vector3 targetPosition = targetTile.transform.position + Vector3.up * transform.localScale.y / 2;

            while (Vector3.SqrMagnitude(targetPosition - transform.position) > 0.001f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, moveTime);
                yield return null;
            }
            
            mapGen.UnclaimTile(currentTile);
            currentTile = targetTile;
        } 

        yield return new WaitForSeconds(2f);
        
        currentState = State.Idle;
        currentRoutine = null;
        moved = true;
    }

    protected override void Die()
    {
        if (targetTile != currentTile)
            mapGen.UnclaimTile(targetTile);

        mapGen.UnclaimTile(currentTile);

        base.Die();
    }
}

