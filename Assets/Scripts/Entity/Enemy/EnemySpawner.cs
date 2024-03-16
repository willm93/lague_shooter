using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool devMode;
    int waveSkipped = 1;
    [SerializeField] Enemy chaserPrefab;
    [SerializeField] Enemy pounderPrefab;
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
        if (devMode && Input.GetKeyDown(KeyCode.N))
        {
            SkipWave();
        }
    }

    void SkipWave()
    {
        if (waveSkipped < waves.Length)
        {
            StopAllCoroutines();

            DestroyRemainingEnemies();

            StartCoroutine(RunWaves(waveSkipped));
            StartCoroutine(AntiCampingTechnology());
            waveSkipped++;
        }
    }

    void DestroyRemainingEnemies()
    {
        foreach(Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            Destroy(enemy.gameObject);
            enemiesRemainingInWave--;
        }
    }
        
    IEnumerator RunWaves(int startingWave = 0)
    {   
        for(int i = startingWave; i < waves.Length; i++)
        {
            OnNewWave?.Invoke(i + 1);
            enemiesRemainingInWave = waves[i].enemyCount;
            ResetPlayerPosition();

            do {
                for(int n = 0; n < waves[i].enemyCount; n++)
                {
                    yield return SpawnEnemy(waves[i], i, n);    
                    yield return new WaitForSeconds(waves[i].timeBetweenSpawns);
                }
            } while (waves[i].infinite);
            
            yield return WaitUntilEnemiesDie();
            waveSkipped++;
            Debug.Log("Wave " + (i + 1) + " Complete");
            yield return new WaitForSeconds(timeBetweenWaves);
            DestroyRemainingEnemies();
        }
    }

    IEnumerator SpawnEnemy(EnemyWave wave, int waveNumber, int enemyNumber)
    {
        GameObject spawnTile = TryFindTile(25);
        
        if (spawnTile)
        {
            Material tileMaterial = spawnTile.GetComponent<Renderer>().material;
            Color initialColor = tileMaterial.color;
            Color newColor = wave.enemyColor;
            
            float spawnDelay = 0.5f;
            float spawnTimer = 0;
            float tileFlashSpeed = 5f;

            while(spawnTimer < spawnDelay)
            {
                tileMaterial.color = Color.Lerp(initialColor, newColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

                spawnTimer += Time.deltaTime;
                yield return null;
            }
            tileMaterial.color = initialColor;

            Enemy enemyPrefab = chaserPrefab;
            if (Random.Range(1, 101) < wave.pounderSpawnChance)
            {
                enemyPrefab = pounderPrefab;
                enemiesRemainingInWave--;
            } 

            Enemy newEnemy = Instantiate(enemyPrefab, spawnTile.transform.position, Quaternion.identity, transform);
            newEnemy.gameObject.name = newEnemy.Name + $" {waveNumber}:{enemyNumber}";
            newEnemy.SetCharacteristics(wave, spawnTile);    

        } else 
        {
            Debug.LogError("Could not find valid spawn point for enemy. Enemy will not be spawned");
        }
    }

    private GameObject TryFindTile(int tries)
    {
        GameObject tile = null;

        if (isCamping)
            tile = mapGen.ClaimTileNearPosition(player.transform.position);

        if (tile) 
            return tile;

        for (int i = 0; i < tries; i++)
        {
            tile = mapGen.ClaimRandomOpenTile();
            if (tile)
                break;
        }

        return tile;
    }

    IEnumerator AntiCampingTechnology()
    {
        while(true){
            yield return new WaitForSeconds(campingCheckInterval);

            distanceFromOldPosition = Vector3.SqrMagnitude(player.transform.position - oldPlayerPosition);
            if (distanceFromOldPosition < thresholdDistanceSqr)
            {
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

    void OnEnemyDeath(bool neededForCount)
    {
        if (neededForCount)
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
