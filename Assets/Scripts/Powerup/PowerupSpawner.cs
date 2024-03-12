using System.Collections;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] Powerup[] powerups;
    [SerializeField] float spawnInterval = 5f;
    MapGenerator mapGen;
    Player player;  

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        mapGen = FindAnyObjectByType<MapGenerator>();
        FindAnyObjectByType<EnemySpawner>().OnNewWave += OnNewWave;

        StartCoroutine(SpawnPowerups());
    }

    void OnNewWave(int waveNumber)
    {
        foreach(Powerup powerup in FindObjectsByType<Powerup>(FindObjectsSortMode.None)){
            Destroy(powerup.gameObject);
        }
    }

    IEnumerator SpawnPowerups()
    {
        while(player != null) {
            yield return new WaitForSeconds(spawnInterval);
            GameObject spawnTile = mapGen.GetRandomOpenTile();
            Powerup randomPowerup = powerups[Random.Range(0, powerups.Length)];
            Instantiate(randomPowerup, spawnTile.transform.position + Vector3.up * 0.6f, Quaternion.identity, transform);
        } 
        
    }
}
