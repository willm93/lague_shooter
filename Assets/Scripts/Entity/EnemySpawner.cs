using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool devMode;
    int waveSkipped = 0;
    public Enemy enemy;
    public float timeBetweenWaves = 5f;
    public Wave[] waves;
    int enemiesRemainingInWave;
    MapGenerator mapGen;  
    Player player;
    public event System.Action<int> OnNewWave;

    //anti-camping technolgies
    public float campingCheckInterval = 4f;
    public float thresholdDistanceSqr = 4f;
    float distanceFromOldPosition;
    Vector3 oldPlayerPosition;
    bool isCamping;


    void Start()
    {
        mapGen = FindAnyObjectByType<MapGenerator>();
        player = FindAnyObjectByType<Player>();
        player.OnDeath += OnPlayerDeath;
        oldPlayerPosition = player.transform.position;

        StartCoroutine(RunWaves());
        StartCoroutine(AntiCampingTechnology());
    }

    void Update()
    {
        if (devMode && Input.GetKeyDown(KeyCode.N)){
            SkipWave();
        }
    }

    void SkipWave()
    {
        StopAllCoroutines();
        waveSkipped++;
        waveSkipped = Mathf.Clamp(waveSkipped, 0, waves.Length - 1);

        foreach(Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None)){
            Destroy(enemy.gameObject);
            enemiesRemainingInWave--;
        }

        StartCoroutine(RunWaves(waveSkipped));
        StartCoroutine(AntiCampingTechnology());
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
            Debug.Log("Wave " + (i + 1) + " Complete");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnEnemy(Wave wave)
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
        Enemy newEnemy = Instantiate<Enemy>(enemy, spawnTile.transform.position + Vector3.up, Quaternion.identity, this.transform);
        newEnemy.SetCharacteristics(wave.enemySpeed, wave.enemyHealth, wave.attackDamage, wave.enemyColor, wave.attackColor);
        newEnemy.OnDeath += OnEnemyDeath;
        
        
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

    [System.Serializable]
    public class Wave 
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;
        public float enemySpeed;
        public int attackDamage;
        public int enemyHealth;
        public Color enemyColor;
        public Color attackColor;
    }
}
