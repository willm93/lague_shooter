using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool devMode;
    int waveSkipped = 1;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] float timeBetweenWaves = 5f;
    [SerializeField] EnemyWave[] waves;
    int enemiesRemainingInWave;
    MapGenerator mapGen;  
    Player player;
    public event System.Action<int> OnNewWave;

    //anti-camping technologies
    [SerializeField] float campingCheckInterval = 4f;
    [SerializeField] float thresholdDistanceSqr = 4f;
    float distanceFromOldPosition;
    Vector3 oldPlayerPosition;
    bool isCamping;

    void Awake()
    {
        mapGen = FindAnyObjectByType<MapGenerator>();
        player = FindAnyObjectByType<Player>();
        player.OnDeath += OnPlayerDeath;
        Enemy.OnDeathStatic += OnEnemyDeath;
        oldPlayerPosition = player.transform.position;
    }
    

    void Start()
    {
        StartCoroutine(RunWaves());
        StartCoroutine(AntiCampingTechnology());
    }
        
    void OnDestroy()
    {
        Enemy.OnDeathStatic -= OnEnemyDeath;
    }
    
    void Update()
    {
        if (devMode && Input.GetKeyDown(KeyCode.N)){
            SkipWave();
        }
    }

    void SkipWave()
    {
        if (waveSkipped < waves.Length){
            StopAllCoroutines();

            foreach(Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None)){
                Destroy(enemy.gameObject);
                enemiesRemainingInWave--;
            }

            StartCoroutine(RunWaves(waveSkipped));
            StartCoroutine(AntiCampingTechnology());
            waveSkipped++;
        }
        
    }

    IEnumerator RunWaves(int startingWave = 0)
    {   
        for(int i = startingWave; i < waves.Length; i++){
            OnNewWave?.Invoke(i + 1);
            enemiesRemainingInWave = waves[i].enemyCount;
            ResetPlayerPosition();

            do {
                for(int n = 0; n < waves[i].enemyCount; n++){
                    yield return SpawnEnemy(waves[i]);    
                    yield return new WaitForSeconds(waves[i].timeBetweenSpawns);
                }
            } while (waves[i].infinite);
            
            yield return WaitUntilEnemiesDie();
            waveSkipped++;
            Debug.Log("Wave " + (i + 1) + " Complete");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnEnemy(EnemyWave wave)
    {
        float spawnDelay = 0.75f;
        float spawnTimer = 0;
        float tileFlashSpeed = 4f;

        GameObject spawnTile;
        if (isCamping){
            spawnTile = mapGen.GetTileNearPosition(player.transform.position);
        } else {
            spawnTile = mapGen.GetRandomOpenTile();
        }
        
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMaterial.color;
        Color newColor = wave.enemyColor;

        while(spawnTimer < spawnDelay){
            tileMaterial.color = Color.Lerp(initialColor, newColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        tileMaterial.color = initialColor;
        Enemy newEnemy = Instantiate(enemyPrefab, spawnTile.transform.position + Vector3.up, Quaternion.identity, transform);
        newEnemy.SetCharacteristics(wave);
    }

    IEnumerator AntiCampingTechnology()
    {
        while(true){
            yield return new WaitForSeconds(campingCheckInterval);

            distanceFromOldPosition = Vector3.SqrMagnitude(player.transform.position - oldPlayerPosition);
            if (distanceFromOldPosition < thresholdDistanceSqr){
                isCamping = true;
            } else {
                isCamping = false;
                oldPlayerPosition = player.transform.position;
            }
        }
    }

    void ResetPlayerPosition()
    {
        player.transform.position = mapGen.GetTileNearPosition(Vector3.zero).transform.position + Vector3.up;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingInWave--;
    }

    void OnPlayerDeath()
    {
        StopAllCoroutines();
    }

    IEnumerator WaitUntilEnemiesDie()
    {
        float refreshRate = 0.25f;
        while (enemiesRemainingInWave > 0)
        {
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
