using System.Collections;
using UnityEngine;

[RequireComponent (typeof (BoxCollider))]
public class PounderWave : MonoBehaviour
{
    public float speed = 1f;
    public int damage = 20;
    int hitCount = 0;
    [SerializeField] LayerMask groundMask;
    [SerializeField] EnemySpawner enemySpawner;


    void Start()
    {
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if(enemySpawner)
            enemySpawner.OnNewWave += OnNewWave;
            
        StartCoroutine(CheckBounds());
    }
    
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnNewWave(int notUsed)
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider collider)
    {
        if(!collider.CompareTag("Enemy"))
        {
            IDamageable damageableObject = collider.GetComponent<IDamageable>();
            damageableObject?.TakeHit(damage, transform.forward);
        }
        Destroy(gameObject);
    }

    void OnDestroy() 
    {
        if(enemySpawner)
            enemySpawner.OnNewWave -= OnNewWave;
    }

    IEnumerator CheckBounds()
    {
        float refreshRate = 0.5f;
        while (true){
            if (!Physics.Raycast(transform.position, -transform.up, 10f, groundMask)){
                Destroy(gameObject);
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}

